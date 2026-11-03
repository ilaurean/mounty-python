import { TestBed } from '@angular/core/testing';
import { Actions } from '@ngrx/effects';
import { Action, provideStore } from '@ngrx/store';
import { firstValueFrom, of, Subject } from 'rxjs';
import { Order, OrderAggregate } from '../../core/models';
import { OrdersApi } from './orders.api';
import { initialOrdersState, ordersActions, ordersFeature, OrdersEffects, toOrderAggregate, upsertOrder } from './orders.store';

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
  const aggregate = (id: string): OrderAggregate => toOrderAggregate(order(id));
  const reducer = ordersFeature.reducer;

  it('stores loaded orders and clears loading', () => {
    const loading = reducer(initialOrdersState, ordersActions.load({ filter: {} }));
    expect(loading.loading).toBe(true);
    const loaded = reducer(loading, ordersActions.loadSuccess({ orders: [aggregate('O1')] }));
    expect(loaded.loading).toBe(false);
    expect(loaded.orders.length).toBe(1);
  });

  it('prepends new orders and replaces existing ones on upsert', () => {
    const added = upsertOrder([aggregate('O1')], order('O2'));
    expect(added.map(o => o.id)).toEqual(['O2', 'O1']);

    const updated = upsertOrder(added, order('O1', 'Processing'));
    expect(updated.find(o => o.id === 'O1')?.status).toBe('Processing');
    expect(updated.length).toBe(2);
  });

  it('applies order upserts through orderUpserted', () => {
    const state = { ...initialOrdersState, orders: [aggregate('O1')] };
    const next = reducer(state, ordersActions.orderUpserted({ order: order('O1', 'Completed') }));
    expect(next.orders[0].status).toBe('Completed');
  });

  it('reloads the current list projection when a SignalR order event arrives', async () => {
    const actions$ = new Subject<Action>();
    const refreshedOrders = [aggregate('O1')];
    const search = vi.fn().mockReturnValue(of(refreshedOrders));
    await TestBed.configureTestingModule({
      providers: [
        OrdersEffects,
        provideStore({ [ordersFeature.name]: ordersFeature.reducer }),
        { provide: Actions, useValue: actions$ },
        { provide: OrdersApi, useValue: { search } }
      ]
    });
    const effect = TestBed.inject(OrdersEffects).orderChanged$;
    const result = firstValueFrom(effect);

    actions$.next(ordersActions.orderChanged());

    expect(await result).toEqual(ordersActions.loadSuccess({ orders: refreshedOrders }));
    expect(search).toHaveBeenCalledWith(initialOrdersState.filter);
  });
});
