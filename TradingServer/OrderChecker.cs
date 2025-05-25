using Trading;
using TradingServer.Orders;
using TradingServer.OrderbookCS;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Security;
using TradingServer.Rejects;

namespace TradingServer.Core
{
    class OrderValidator
    {
        public OrderValidator()
        {
            _validators = new Dictionary<RejectionReason, IValidator>();

            // _validators.Add(key: RejectionReason.InsufficientPermissionError, value: new PermissionValidator(level));
            _validators.Add(key: RejectionReason.SubmittedAfterDeadline, value: new DeadlineValidator());
            _validators.Add(key: RejectionReason.EmptyOrNullArgument, value: new EmptyOrNullArgumentValidator());
            _validators.Add(key: RejectionReason.InvalidOrUnknownArgument, value: new InvalidArgumentValidator());
            _validators.Add(key: RejectionReason.OperationNotFound, value: new OperationValidator());
        }

        public RejectionReason Check(OrderRequest request)
        {
            foreach (var pair in _validators)
            {
                if (pair.Value.Check(request))
                {
                    return pair.Key;
                }
            }

            return RejectionReason.None;
        }

        private readonly Dictionary<RejectionReason, IValidator> _validators;
    }

    public class InvalidArgumentValidator: IValidator
    {
        public bool Check(OrderRequest request)
        {
            return request.Side.ToString() != "Bid" && request.Side.ToString() != "Ask";
        }
    }

    public class EmptyOrNullArgumentValidator: IValidator
    {
        public bool Check(OrderRequest request)
        {
            return Order.StringToOrderType(request.Type) == OrderTypes.Market &&
                (!string.IsNullOrWhiteSpace(request.Price.ToString())
                 || !string.IsNullOrWhiteSpace(request.Operation)
                 || string.IsNullOrWhiteSpace(request.Id.ToString())
                || string.IsNullOrWhiteSpace(request.Username));
        }
    }

    public class DeadlineValidator: IValidator
    {
        public bool Check(OrderRequest request)
        {
            TimeSpan now = DateTime.Now.TimeOfDay;

            return ((Order.StringToOrderType(request.Type) == OrderTypes.MarketOnClose) ||
                Order.StringToOrderType(request.Type) == OrderTypes.LimitOnClose) && (now >= onCloseDeadline)
                || Order.StringToOrderType(request.Type) == OrderTypes.MarketOnOpen ||
                Order.StringToOrderType(request.Type) == OrderTypes.LimitOnOpen && (now >= onOpenDeadline)
                || (now >= closed);
        }

        private readonly TimeSpan onOpenDeadline = new TimeSpan(9, 28, 0);
        private readonly TimeSpan onCloseDeadline = new TimeSpan(15, 50, 0);
        private readonly TimeSpan open = new TimeSpan(9, 30, 0);
        private readonly TimeSpan closed = new TimeSpan(16, 0, 0);

    }

    public class OperationValidator : IValidator
    {
        public bool Check(OrderRequest request)
        {
            return (Order.StringToOrderType(request.Type) == OrderTypes.FillAndKill
                || (Order.StringToOrderType(request.Type) == OrderTypes.FillOrKill))
                && (!string.IsNullOrEmpty(request.Operation));
        }
    }

    public class PermissionValidator : IValidator
    {
        public PermissionValidator(PermissionLevel _permissionLevel)
        {
            permissionLevel = _permissionLevel;
        }

        private PermissionLevel permissionLevel;
        public bool Check(OrderRequest request) => (int)permissionLevel < 2;
    }

    public interface IValidator
    {
        bool Check(OrderRequest request);
    }
}