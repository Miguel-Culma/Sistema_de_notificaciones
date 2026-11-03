import { Navigate, Route, Routes } from 'react-router-dom'
import ProtectedRoute from './components/ProtectedRoute.jsx'
import AdminRoute from './components/AdminRoute.jsx'
import MainLayout from './layout/MainLayout.jsx'
import Login from './pages/Login.jsx'
import Dashboard from './pages/Dashboard.jsx'
import Invoices from './pages/Invoices.jsx'
import InvoiceDetail from './pages/InvoiceDetail.jsx'
import Notifications from './pages/Notifications.jsx'
import ReminderSettings from './pages/ReminderSettings.jsx'
import NotFound from './pages/NotFound.jsx'

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<Login />} />

      <Route element={<ProtectedRoute />}>
        <Route element={<MainLayout />}>
          <Route index element={<Dashboard />} />
          <Route path="invoices" element={<Invoices />} />
          <Route path="invoices/:id" element={<InvoiceDetail />} />
          <Route path="notifications" element={<Notifications />} />

          {/* Solo visible y accesible si el JWT trae el rol admin */}
          <Route element={<AdminRoute />}>
            <Route path="settings" element={<ReminderSettings />} />
          </Route>

          <Route path="home" element={<Navigate to="/" replace />} />
          <Route path="*" eleme nt={<NotFound />} />
        </Route>
      </Route>
    </Routes>
  )
}
