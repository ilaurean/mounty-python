import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Board, BoardFilter, CreateBoardRequest, UpdateBoardRequest } from '../../../core/models';

@Injectable({ providedIn: 'root' })
export class BoardsApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/catalogs/boards';

  search(filter: BoardFilter): Observable<Board[]> {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(filter)) {
      if (value !== undefined && value !== null && value !== '') {
        params = params.set(key, String(value));
      }
    }
    return this.http.get<Board[]>(this.baseUrl, { params });
  }

  create(request: CreateBoardRequest): Observable<Board> {
    return this.http.post<Board>(this.baseUrl, request);
  }

  update(id: string, request: UpdateBoardRequest): Observable<Board> {
    return this.http.put<Board>(`${this.baseUrl}/${encodeURIComponent(id)}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${encodeURIComponent(id)}`);
  }
}
