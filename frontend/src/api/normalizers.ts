import type {
  Carrier,
  Category,
  Governorate,
  Offer,
  Order,
  Product,
  ShippingRate,
  User,
} from '../types';

// The API uses entity-specific identifiers (productId, orderId, etc.), while
// the existing UI uses a shared `id` property. Keep that translation here so
// pages do not each need their own version of the same mapping.
export const toProduct = (value: any): Product => ({
  ...value,
  id: value.productId,
  hasOffer: value.hasActiveOffer,
  images: (value.images ?? []).map((image: any) => ({ ...image, id: image.imageId })),
  pricingTiers: (value.pricingTiers ?? []).map((tier: any) => ({ ...tier, id: tier.tierId })),
});

export const toProducts = (values: any[] = []): Product[] => values.map(toProduct);

export const toCategory = (value: any): Category => ({ ...value, id: value.categoryId });
export const toCategories = (values: any[] = []): Category[] => values.map(toCategory);

export const toGovernorate = (value: any): Governorate => ({ ...value, id: value.governorateId });
export const toGovernorates = (values: any[] = []): Governorate[] => values.map(toGovernorate);

export const toCarrier = (value: any): Carrier => ({
  ...value,
  id: value.carrierId,
  phone: value.phone ?? '',
});
export const toCarriers = (values: any[] = []): Carrier[] => values.map(toCarrier);

export const toShippingRate = (value: any): ShippingRate => ({
  ...value,
  id: value.shippingRateId,
  rate: value.cost,
});
export const toShippingRates = (values: any[] = []): ShippingRate[] => values.map(toShippingRate);

export const toOrder = (value: any): Order => ({
  ...value,
  id: value.orderId,
  orderStatus: value.status,
  orderItems: (value.orderItems ?? []).map((item: any) => ({ ...item, id: item.orderItemId })),
});
export const toOrders = (values: any[] = []): Order[] => values.map(toOrder);

export const toOffer = (value: any): Offer => ({ ...value, id: value.offerId });
export const toOffers = (values: any[] = []): Offer[] => values.map(toOffer);

export const toUser = (value: any, storeId: number | null = null): User => ({
  ...value,
  phone: value.phone ?? '',
  firstName: value.firstName ?? '',
  lastName: value.lastName ?? '',
  storeId,
});
