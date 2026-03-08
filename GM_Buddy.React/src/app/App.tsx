import { BrowserRouter, Routes, Route } from "react-router-dom";
import { AuthProvider } from "@/contexts/AuthContext";
import { Toaster } from "@/app/components/ui/sonner";
import { HomePage } from "@/app/pages/HomePage";
import { RelationshipManagerPage } from "@/app/pages/RelationshipManagerPage";
import { CampaignPage } from "@/app/pages/CampaignPage";
import { AccountPage } from "@/app/pages/AccountPage";
import { AboutPage } from "@/app/pages/AboutPage";
import { ProtectedRoute } from "@/app/components/ProtectedRoute";

console.log('[App] GM Buddy React App loading - v8');

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Toaster />
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route
            path="/relationship-manager"
            element={
              <ProtectedRoute>
                <RelationshipManagerPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/campaign/:id"
            element={
              <ProtectedRoute>
                <CampaignPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/account"
            element={
              <ProtectedRoute>
                <AccountPage />
              </ProtectedRoute>
            }
          />
          <Route path="/about" element={<AboutPage />} />
          <Route path="/callback" element={<HomePage />} />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  );
}
