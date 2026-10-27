import { HttpClient, HttpParams, HttpResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { CreateOrderRequest, Order, OrderFilter, UpdateOrderRequest } from '../../core/models';

@Injectable({ providedIn: 'root' })
export class OrdersApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/operations/orders';

  search(filter: OrderFilter): Observable<Order[]> {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(filter)) {
      if (value !== undefined && value !== null && value !== '') {
        params = params.set(key, String(value));
      }
    }
    return this.http.get<Order[]>(this.baseUrl, { params });
  }

  create(request: CreateOrderRequest): Observable<Order> {
    return this.http.post<Order>(this.baseUrl, request);
  }

  update(id: string, request: UpdateOrderRequest): Observable<Order> {
    return this.http.put<Order>(this.url(id), request);
  }

  startProcessing(id: string): Observable<Order> {
    return this.http.post<Order>(`${this.url(id)}/start-processing`, {});
  }

  complete(id: string): Observable<Order> {
    return this.http.post<Order>(`${this.url(id)}/complete`, {});
  }

  cancel(id: string): Observable<Order> {
    return this.http.post<Order>(`${this.url(id)}/cancel`, {});
  }

  download(id: string): Observable<HttpResponse<Blob>> {
    return this.http.get(`${this.url(id)}/download`, { responseType: 'blob', observe: 'response' });
  }

  private url(id: string): string {
    return `${this.baseUrl}/${encodeURIComponent(id)}`;
  }
}
