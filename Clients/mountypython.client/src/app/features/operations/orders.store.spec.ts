import { Order } from '../../core/models';
import { initialOrdersState, ordersActions, ordersFeature, upsertOrder } from './orders.store';

const order = (id: string, status: Order['status'] = 'Created'): Order => ({
  pk: `pk-${id}`,
  id,
  customerID: 'TestUser1',
  name: id,
  items: [],
  orderDate: '2026-01-01T00:00:00Z',
  status
});

describe('orders store', () => {
  const reducer = ordersFeature.reducer;

  it('stores loaded orders and clears loading', () => {
    const loading = reducer(initialOrdersState, ordersActions.load({ filter: {} }));
    expect(loading.loading).toBe(true);
    const loaded = reducer(loading, ordersActions.loadSuccess({ orders: [order('O1')] }));
    expect(loaded.loading).toBe(false);
    expect(loaded.orders.length).toBe(1);
  });

  it('prepends new orders and replaces existing ones on upsert', () => {
    const added = upsertOrder([order('O1')], order('O2'));
    expect(added.map(o => o.id)).toEqual(['O2', 'O1']);

    const updated = upsertOrder(added, order('O1', 'Processing'));
    expect(updated.find(o => o.id === 'O1')?.status).toBe('Processing');
    expect(updated.length).toBe(2);
  });

  it('applies SignalR order updates through orderUpserted', () => {
    const state = { ...initialOrdersState, orders: [order('O1')] };
    const next = reducer(state, ordersActions.orderUpserted({ order: order('O1', 'Completed') }));
    expect(next.orders[0].status).toBe('Completed');
  });
});
