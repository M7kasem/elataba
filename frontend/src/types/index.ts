export enum Role {
  Buyer = 0,
  Seller = 1,
  Admin = 2,
  StoreManager = 3,
}

export enum PaymentMethod {
  Cash = 0,
  Online = 1,
}

export enum OrderStatus {
  Pending = 0,
  Confirmed = 1,
  Shipped = 2,
  Delivered = 3,
  Cancelled = 4,
}

export enum PaymentStatus {
  Pending = 0,
  Paid = 1,
  Failed = 2,
}

export interface User {
  userId: number;
  email: string;
  firstName: string;
  lastName: string;
  phone: string;
  role: Role;
  governorateId: number;
  governorateName?: string;
  city: string;
  shippingAddress: string;
  storeId?: number | null;
}

export interface AuthState {
  token: string | null;
  userId: number | null;
  email: string | null;
  role: Role | null;
  storeId: number | null;
}

export interface Category {
  id: number;
  name: string;
  description?: string;
}

export interface Governorate {
  id: number;
  name: string;
  shippingCost?: number; // fallback or general shipping cost
}

export interface Carrier {
  id: number;
  name: string;
  phone: string;
}

export interface ShippingRate {
  id: number;
  governorateId: number;
  governorateName?: string;
  carrierId: number;
  carrierName?: string;
  rate: number;
}

export interface PricingTier {
  id?: number;
  productId?: number;
  minQuantity: number;
  pricePerUnit: number;
}

export interface ProductImage {
  id: number;
  imageUrl: string;
  isPrimary: boolean;
}

export interface Product {
  id: number;
  storeId: number;
  storeName: string;
  categoryId: number;
  categoryName: string;
  name: string;
  description: string;
  basePrice: number;
  oldPrice?: number;
  currentPrice: number;
  discountPercentage?: number;
  stockQuantity: number;
  hasOffer: boolean;
  hasActiveOffer: boolean;
  images: ProductImage[];
  pricingTiers: PricingTier[];
}

export interface CartItem {
  product: Product;
  quantity: number;
  selectedPrice: number; // updates dynamically based on pricing tiers
}

export interface OrderItem {
  id?: number;
  productId: number;
  productName: string;
  productImageUrl?: string;
  quantity: number;
  unitPrice: number;
  subtotal: number;
}

export interface Order {
  id: number;
  buyerId: number;
  buyerEmail?: string;
  storeId: number;
  storeName: string;
  carrierId?: number | null;
  carrierName?: string;
  orderDate: string;
  totalAmount: number;
  shippingCost: number;
  shippingAddressSnapshot: string;
  trackingNumber?: string | null;
  paymentMethod: PaymentMethod;
  paymentStatus: PaymentStatus;
  orderStatus: OrderStatus;
  orderItems: OrderItem[];
  isReviewed?: boolean;
}

export interface Review {
  id: number;
  buyerName: string;
  rating: number;
  comment: string;
  createdAt: string;
  productId: number;
  orderId: number;
}

export interface Message {
  id: number;
  senderId: number;
  senderEmail: string;
  recipientId: number;
  recipientEmail: string;
  productId?: number;
  productName?: string;
  content: string;
  timestamp: string;
}

export interface Offer {
  id: number;
  storeId: number;
  discountPercentage: number;
  startDate: string;
  endDate: string;
  appliesToAllProducts: boolean;
  productIds?: number[];
  productNames?: string[];
}

export interface CheckoutOrderDto {
  orderId: number;
  storeId: number;
  storeName?: string;
  totalAmount: number;
  itemCount: number;
}

export interface CheckoutResultDto {
  checkoutReference: string;
  totalAmount: number;
  orders: CheckoutOrderDto[];
}
