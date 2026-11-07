import { createBrowserRouter, Navigate } from 'react-router-dom';
import { LoginPage } from '@/features/auth/LoginPage';
import { RegisterPage } from '@/features/auth/RegisterPage';
import { AppShell } from './AppShell';
import { NotFoundPage } from './NotFoundPage';
import { PublicOnly } from './PublicOnly';
import { RequireAuth } from './RequireAuth';
import { RouteError } from './RouteError';

export const router = createBrowserRouter([
  {
    errorElement: <RouteError />,
    children: [
      { path: '/login', element: <PublicOnly><LoginPage /></PublicOnly> },
      { path: '/register', element: <PublicOnly><RegisterPage /></PublicOnly> },
      {
        // Everything below needs a signed-in merchant.
        element: <RequireAuth />,
        children: [
          {
            element: <AppShell />,
            children: [
              { index: true, element: <Navigate to="/products" replace /> },
              { path: 'products', lazy: async () => ({ Component: (await import('@/features/products/ProductsPage')).ProductsPage }) },
              { path: 'products/:id', lazy: async () => ({ Component: (await import('@/features/products/ProductDetailPage')).ProductDetailPage }) },
              { path: 'pools', lazy: async () => ({ Component: (await import('@/features/pools/MyPoolsPage')).MyPoolsPage }) },
              { path: 'pools/new', lazy: async () => ({ Component: (await import('@/features/pools/NewPoolPage')).NewPoolPage }) },
              { path: 'pools/:id', lazy: async () => ({ Component: (await import('@/features/pools/PoolDetailPage')).PoolDetailPage }) },
              { path: 'cart', lazy: async () => ({ Component: (await import('@/features/cart/CartPage')).CartPage }) },
              { path: 'checkout', lazy: async () => ({ Component: (await import('@/features/cart/CheckoutPage')).CheckoutPage }) },
              { path: 'orders', lazy: async () => ({ Component: (await import('@/features/orders/OrdersPage')).OrdersPage }) },
              { path: 'orders/:id', lazy: async () => ({ Component: (await import('@/features/orders/OrderDetailPage')).OrderDetailPage }) },
              // The admin screens are not built yet. The guard still applies, so signed-out visitors are sent to /login first.
              { path: 'admin/*', element: <NotFoundPage /> },
            ],
          },
        ],
      },
      { path: '*', element: <NotFoundPage /> },
    ],
  },
]);
