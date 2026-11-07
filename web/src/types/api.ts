// Mirrors the C# DTOs in src/Application and the request records in src/Api, one to one.
// The API sends camelCase JSON, enums as strings, Guids as strings and DateTimeOffset as ISO 8601 strings.

// Enums (src/Domain/Enums)

export const BULK_POOL_STATUSES = ['Open', 'ThresholdMet', 'Locked', 'Finalized', 'Expired', 'Cancelled'] as const;
export type BulkPoolStatus = (typeof BULK_POOL_STATUSES)[number];

export const ORDER_STATUSES = ['Pending', 'Paid', 'Shipped', 'Delivered', 'Cancelled'] as const;
export type OrderStatus = (typeof ORDER_STATUSES)[number];

export const MERCHANT_ROLES = ['Merchant', 'Wholesaler', 'Admin'] as const;
export type MerchantRole = (typeof MERCHANT_ROLES)[number];

export const PRODUCT_CATEGORIES = ['MaizeMeal', 'CookingOil', 'Rice', 'Sugar', 'SoftDrinks', 'Flour'] as const;
export type ProductCategory = (typeof PRODUCT_CATEGORIES)[number];

export const UNITS_OF_MEASURE = ['Each', 'Kilogram', 'Litre', 'Box', 'Bag'] as const;
export type UnitOfMeasure = (typeof UNITS_OF_MEASURE)[number];

// Shared

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

/** RFC 7807 problem details. Validation failures from [ApiController] also carry `errors`. */
export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  errors?: Record<string, string[]>;
}

// Products (Application/Products/SearchProduct/ProductDto.cs)

export interface DiscountTierDto {
  minimumQuantity: number;
  discountPercentage: number;
}

export interface ProductDto {
  productId: string;
  name: string;
  sku: string;
  basePrice: number;
  category: ProductCategory;
  unitOfMeasure: UnitOfMeasure;
  /** Sorted ascending by minimumQuantity. */
  discountTiers: DiscountTierDto[];
}

// Pools (Application/BulkPools/BulkPoolDto.cs)

export interface BulkPoolDto {
  poolId: string;
  productId: string;
  status: BulkPoolStatus;
  currentQuantity: number;
  /** The caller's own quantity in the pool, 0 if they have not joined. */
  myQuantity: number;
  deadline: string;
  minimumQuantity: number;
  minimumMerchants: number;
  participantCount: number;
}

export interface CreatePoolRequest {
  quantity: number;
  productId: string;
  minimumQuantity: number;
  minimumMerchants: number;
  deadline: string;
}

export interface CreatePoolResult {
  poolId: string;
  status: BulkPoolStatus;
  createdAt: string;
}

export interface JoinPoolRequest {
  quantity: number;
}

export interface PoolQuantityResult {
  status: BulkPoolStatus;
  currentQuantity: number;
}

/** Result of lock, finalise, expire and cancel. */
export interface PoolStatusResult {
  poolId: string;
  status: BulkPoolStatus;
}

// Cart (Application/Carts)

export interface CartItemDto {
  productId: string;
  quantity: number;
}

export interface CartDto {
  /** The empty Guid when the merchant has no cart yet. */
  cartId: string;
  merchantId: string;
  cartItems: CartItemDto[];
  totalQuantity: number;
}

export interface AddToCartRequest {
  productId: string;
  quantity: number;
}

export interface UpdateQuantityRequest {
  quantity: number;
}

// Orders (Application/Orders)

export interface OrderLineDto {
  productId: string;
  quantity: number;
  unitPrice: number;
}

export interface OrderDto {
  orderId: string;
  merchantId: string;
  orderStatus: OrderStatus;
  totalAmount: number;
  createdAt: string;
  refundId: string | null;
  paymentId: string | null;
  orderLines: OrderLineDto[];
}

export interface PayOrderRequest {
  cardToken: string;
}

export interface PayOrderResult {
  orderStatus: OrderStatus;
  transactionId: string | null;
}

export interface CancelOrderResult {
  orderId: string;
  status: OrderStatus;
  refundTransactionId: string | null;
}

export interface OrderStatusResult {
  orderId: string;
  status: OrderStatus;
}

// Merchants (Application/Merchants and Api/Controllers/Merchants)

export interface RegisterMerchantRequest {
  name: string;
  phoneNumber: string;
  password: string;
  street: string;
  city: string;
  province: string;
  postalCode: string;
  country: string;
  suburb?: string;
}

export interface RegisterMerchantResult {
  merchantId: string;
}

export interface LoginMerchantRequest {
  phoneNumber: string;
  password: string;
}

export interface LoginMerchantResult {
  token: string;
}
