using System;
using System.Collections.Generic;
using BuildItEasy.States;
using BuildItEasy.Tests.Sample.Domain;

namespace BuildItEasy.Tests.Sample.Builders
{
    public class OrderBuilder : Builder<Order, OrderBuilder>
    {
        private static readonly Identity<string> OrderNumberIdentity
            = new Identity<string>(i => $"{DateTime.Today:yyyy}/{i:000000}");

        private readonly Property<string> _orderNumber;
        private readonly ChildBuilder<Contact, ContactBuilder> _contact = Child<Contact, ContactBuilder>(o => new ContactBuilder(o));
        private readonly Property<string> _paymentTransactionId;
        private readonly Property<OrderCancellationReason> _cancellationReason;

//        private readonly Property<IReadOnlyList<Action<Order>>> _orderLines
//            = new Property<IReadOnlyList<Action<Order>>>(new Action<Order>[] {o => new OrderLineBuilder(o).Build()});


        private readonly StateHelper<Order, OrderState, OrderBuilder> _state = State<OrderState, OrderStateDefinition>();

        public OrderBuilder()
        {
            _orderNumber = Property(o => o.OrderNumber, OrderNumberIdentity).Required();
            _paymentTransactionId = Property(o => o.PaymentTransactionId, "1234").OnlyIfNecessary();
            _cancellationReason = Property<OrderCancellationReason>("CancellationReason", OrderCancellationReason.Other).OnlyIfNecessary();
        }
        
        // Data
        
        public OrderBuilder WithOrderNumber(string orderNumber) => SetValue(_orderNumber, orderNumber);

        public OrderBuilder WithContact(Action<ContactBuilder> customize) => CustomizeChild(_contact, customize);
        public OrderBuilder WithoutContact() => NoChild(_contact);

//        public OrderBuilder WithOrderLine(Action<OrderLineBuilder> customize = null)
//            => SetValue(_orderLines, new Action<Order>[] {o => new OrderLineBuilder(o).Customize(customize).Build()});

        public OrderBuilder WithNumberOfOrderLines(int count) => this;
        public OrderBuilder WithOrderLines(params Action<OrderLineBuilder>[] customizes) => this;
        public OrderBuilder WithoutOrderLines() => WithNumberOfOrderLines(0);

        // Check Out

        private OrderBuilder PrepareForCheckOut()
        {
            _contact.EnsureChild();
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

            BuildChild(order, _contact).IfSome(c => order.SetContact(c));

            new OrderLineBuilder(order).Build(); //TODO
//            foreach (var action in _orderLines.GetValue())
//                action.Invoke(order);

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
