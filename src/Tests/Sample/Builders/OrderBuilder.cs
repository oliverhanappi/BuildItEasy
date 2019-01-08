using System;
using BuildItEasy.Identities;
using BuildItEasy.States;
using BuildItEasy.Tests.Sample.Domain;

namespace BuildItEasy.Tests.Sample.Builders
{
    public class OrderBuilder : Builder<Order, OrderBuilder>
    {
        private static readonly Identity<string> OrderNumberIdentity
            = new Identity<string>(i => $"{DateTime.Today:yyyy}/{i:000000}");

        private readonly Value<string> _orderNumber;
        private readonly Child<Contact, ContactBuilder> _contact;
        private readonly Children<OrderLine, OrderLineBuilder> _orderLines;
        private readonly Value<string> _paymentTransactionId;
        private readonly Value<OrderCancellationReason> _cancellationReason;

        private readonly StateHelper<Order, OrderState, OrderBuilder> _state = State<OrderState, OrderStateDefinition>();

        public OrderBuilder()
        {
            _orderNumber = Value(o => o.OrderNumber, OrderNumberIdentity).Required();
            _contact = AddChild(o => o.Contact, o => new ContactBuilder(o)).Required();
            _paymentTransactionId = Value(o => o.PaymentTransactionId, "1234").OnlyIfNecessary();
            _cancellationReason = Value<OrderCancellationReason>("CancellationReason", OrderCancellationReason.Other).OnlyIfNecessary();
            _orderLines = AddChildren(o => o.OrderLines, (o, _) => new OrderLineBuilder(o));
        }
        
        // Data
        
        public OrderBuilder WithOrderNumber(ValueProvider<string> orderNumber) => SetValue(_orderNumber, orderNumber);

        public OrderBuilder WithContact(Customizer<ContactBuilder> customizer) => CustomizeChild(_contact, customizer);
        public OrderBuilder WithoutContact() => NoChild(_contact);

        public OrderBuilder WithOrderLines(ValuesCustomizer<OrderLine, OrderLineBuilder> customizer) => CustomizeChildren(_orderLines, customizer);

        // Check Out

        private OrderBuilder PrepareForCheckOut()
        {
            _contact.EnsureValue();
            return this;
        }

        public OrderBuilder SuitableForCheckOut()
        {
            PrepareForCheckOut();
            _state.RequireExactly(OrderState.Open);
            return this;
        }

        public OrderBuilder CheckedOut()
        {
            PrepareForCheckOut();
            _state.RequireAtLeast(OrderState.PaymentPending);
            return this;
        }

        public OrderBuilder NotCheckedOut()
        {
            _state.RequireAtMost(OrderState.Open);
            return this;
        }

        // Payment

        private OrderBuilder PrepareForConfirmingPayment() => CheckedOut();

        public OrderBuilder SuitableForConfirmingPayment()
        {
            PrepareForConfirmingPayment();
            _state.RequireExactly(OrderState.PaymentPending);
            return this;
        }

        public OrderBuilder Paid()
        {
            PrepareForConfirmingPayment();
            _paymentTransactionId.EnsureValue();
            _state.RequireAtLeast(OrderState.ShipmentPending);
            return this;
        }

        public OrderBuilder NotPaid()
        {
            _state.RequireAtMost(OrderState.PaymentPending);
            return this;
        }

        // Shipment

        private OrderBuilder PrepareForShipping() => Paid();

        public OrderBuilder SuitableForShipping()
        {
            PrepareForShipping();
            _state.RequireExactly(OrderState.ShipmentPending);
            return this;
        }

        public OrderBuilder Shipped()
        {
            PrepareForShipping();
            _state.RequireAtLeast(OrderState.Shipped);
            return this;
        }

        public OrderBuilder NotShipped()
        {
            _state.RequireAtMost(OrderState.ShipmentPending);
            return this;
        }
        
        // Cancellation

        public OrderBuilder WithCancellationReason(OrderCancellationReason cancellationReason) =>
            SetValue(_cancellationReason, cancellationReason);
        
        private OrderBuilder PrepareForCancellation() => this;
        
        public OrderBuilder SuitableForCancellation() => PrepareForCancellation();

        public OrderBuilder Canceled()
        {
            PrepareForCancellation();
            _state.RequireAtLeast(OrderState.Canceled);
            _cancellationReason.EnsureValue();
            return this;
        }

        public OrderBuilder NotCanceled()
        {
            _state.EnsureNot(OrderState.Canceled);
            return this;
        }

        protected override Order BuildInternal()
        {
            var order = new Order(_orderNumber);

            BuildChild(order, _contact);
            BuildChildren(order, _orderLines);

            Transition(order, _state);

            return order;
        }

        private class OrderStateDefinition : EnumStateDefinition<Order, OrderState, OrderBuilder>
        {
            public override OrderState GetInitialState(Order order, OrderBuilder builder) => OrderState.Open;
            public override OrderState GetState(Order order) => order.State;

            protected override void ConfigureTransitions()
            {
                Transition(OrderState.Open, OrderState.PaymentPending, o => o.CheckOut());
                Transition(OrderState.PaymentPending, OrderState.ShipmentPending, (o, b) => o.ConfirmPayment(b._paymentTransactionId));
                Transition(OrderState.ShipmentPending, OrderState.Shipped, o => o.Ship());

                Transition(OrderState.Open, OrderState.Canceled, Cancel);
                Transition(OrderState.PaymentPending, OrderState.Canceled, Cancel);
                Transition(OrderState.ShipmentPending, OrderState.Canceled, Cancel);
                Transition(OrderState.Shipped, OrderState.Canceled, Cancel);

                void Cancel(Order order, OrderBuilder builder) => order.Cancel(builder._cancellationReason);
            }
        }
    }
}
