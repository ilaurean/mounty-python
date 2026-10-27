export interface Auditable {
  pk: string;
  id: string;
  createDate?: string | null;
  updateDate?: string | null;
  deleteDate?: string | null;
}

export interface Component {
  pk?: string;
  name: string;
  description?: string | null;
  quantity: number;
}

export interface Board extends Auditable {
  name: string;
  description?: string | null;
  length?: number | null;
  width?: number | null;
  components: Component[];
}

export interface BoardFilter {
  boardID?: string;
  name?: string;
  pageIndex?: number;
  pageSize?: number;
}

export interface ComponentRequest {
  name: string;
  description?: string | null;
  quantity: number;
}

export interface CreateBoardRequest {
  id: string;
  name: string;
  description?: string | null;
  length?: number | null;
  width?: number | null;
  components: ComponentRequest[];
}

export type UpdateBoardRequest = Omit<CreateBoardRequest, 'id'>;

export type OrderStatus = 'Created' | 'Processing' | 'Completed' | 'Cancelled';

export const ORDER_STATUSES: OrderStatus[] = ['Created', 'Processing', 'Completed', 'Cancelled'];

export interface OrderItem {
  pk: string;
  position: number;
  board: Board;
  quantity: number;
}

export interface Order extends Auditable {
  customerID: string;
  name: string;
  description?: string | null;
  items: OrderItem[];
  orderDate: string;
  status: OrderStatus;
}

export interface OrderFilter {
  orderID?: string;
  name?: string;
  customerID?: string;
  state?: OrderStatus;
  boardID?: string;
  pageIndex?: number;
  pageSize?: number;
}

export interface OrderItemComponentRequest {
  name: string;
  quantity: number;
}

export interface CreateOrderItemRequest {
  boardID: string;
  boardName: string;
  quantity: number;
  components: OrderItemComponentRequest[];
}

export interface AuthTokens {
  token?: string;
  refreshToken?: string;
  idToken?: string;
}

export interface BoardSearch {
  name: string;
  boardID: string;
}

export interface OrderSearch {
  name: string;
  state: OrderStatus | '';
}

export interface BoardDetailsData {
  board: Board;
}

export interface OrderDetailsData {
  order: Order;
}

export interface CreateOrderRequest {
  name: string;
  description?: string | null;
  orderDate: string;
  items: CreateOrderItemRequest[];
}

export interface UpdateOrderRequest {
  name?: string | null;
  description?: string | null;
}
