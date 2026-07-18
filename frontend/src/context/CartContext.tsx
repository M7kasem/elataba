import React, { createContext, useContext, useState, useEffect, useRef } from 'react';
import { Product, CartItem } from '../types';
import { useAuth } from './AuthContext';

interface CartContextType {
  cartItems: CartItem[];
  addToCart: (product: Product, quantity: number) => void;
  updateQuantity: (productId: number, quantity: number) => void;
  removeFromCart: (productId: number) => void;
  clearCart: () => void;
  getGroupedCart: () => { [storeName: string]: CartItem[] };
  getCartTotal: () => number;
  getItemPrice: (product: Product, quantity: number) => number;
}

const CartContext = createContext<CartContextType | undefined>(undefined);

const getCartKey = () => {
  const storedUserId = localStorage.getItem('elAtaba_userId');
  return storedUserId ? `elAtaba_cart_${storedUserId}` : 'elAtaba_cart_guest';
};

export const CartProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { userId } = useAuth();
  const [cartItems, setCartItems] = useState<CartItem[]>(() => {
    const key = getCartKey();
    const savedCart = localStorage.getItem(key);
    return savedCart ? JSON.parse(savedCart) : [];
  });
  const loadedUserIdRef = useRef<number | null | undefined>(undefined);

  useEffect(() => {
    const key = userId ? `elAtaba_cart_${userId}` : 'elAtaba_cart_guest';
    const savedCart = localStorage.getItem(key);
    setCartItems(savedCart ? JSON.parse(savedCart) : []);
    loadedUserIdRef.current = userId;
  }, [userId]);

  useEffect(() => {
    if (loadedUserIdRef.current === userId) {
      const key = userId ? `elAtaba_cart_${userId}` : 'elAtaba_cart_guest';
      localStorage.setItem(key, JSON.stringify(cartItems));
    }
  }, [cartItems, userId]);

  // Pricing tier calculator: checks breakpoints and active offer price
  const getItemPrice = (product: Product, quantity: number): number => {
    // 1. Check if there are quantity tiers defined for this product
    if (product.pricingTiers && product.pricingTiers.length > 0) {
      // Sort tiers descending by minQuantity to find the highest matched breakpoint
      const sortedTiers = [...product.pricingTiers].sort((a, b) => b.minQuantity - a.minQuantity);
      const matchedTier = sortedTiers.find((tier) => quantity >= tier.minQuantity);
      if (matchedTier) {
        return matchedTier.pricePerUnit;
      }
    }
    // 2. Fallback to product's currentPrice (calculated with active offer by backend)
    return product.currentPrice;
  };

  const addToCart = (product: Product, quantity: number) => {
    setCartItems((prevItems) => {
      const existingItemIndex = prevItems.findIndex((item) => item.product.id === product.id);

      if (existingItemIndex > -1) {
        const newQty = prevItems[existingItemIndex].quantity + quantity;
        const updatedItems = [...prevItems];
        updatedItems[existingItemIndex] = {
          product,
          quantity: newQty,
          selectedPrice: getItemPrice(product, newQty),
        };
        return updatedItems;
      } else {
        return [
          ...prevItems,
          {
            product,
            quantity,
            selectedPrice: getItemPrice(product, quantity),
          },
        ];
      }
    });
  };

  const updateQuantity = (productId: number, quantity: number) => {
    if (quantity <= 0) {
      removeFromCart(productId);
      return;
    }

    setCartItems((prevItems) =>
      prevItems.map((item) =>
        item.product.id === productId
          ? {
              ...item,
              quantity,
              selectedPrice: getItemPrice(item.product, quantity),
            }
          : item
      )
    );
  };

  const removeFromCart = (productId: number) => {
    setCartItems((prevItems) => prevItems.filter((item) => item.product.id !== productId));
  };

  const clearCart = () => {
    setCartItems([]);
  };

  // Group items by storeName for visual split in Cart and Checkout
  const getGroupedCart = () => {
    const groups: { [storeName: string]: CartItem[] } = {};
    cartItems.forEach((item) => {
      const storeName = item.product.storeName || 'General Store';
      if (!groups[storeName]) {
        groups[storeName] = [];
      }
      groups[storeName].push(item);
    });
    return groups;
  };

  const getCartTotal = () => {
    return cartItems.reduce((acc, item) => acc + item.selectedPrice * item.quantity, 0);
  };

  return (
    <CartContext.Provider
      value={{
        cartItems,
        addToCart,
        updateQuantity,
        removeFromCart,
        clearCart,
        getGroupedCart,
        getCartTotal,
        getItemPrice,
      }}
    >
      {children}
    </CartContext.Provider>
  );
};

export const useCart = () => {
  const context = useContext(CartContext);
  if (context === undefined) {
    throw new Error('useCart must be used within a CartProvider');
  }
  return context;
};
