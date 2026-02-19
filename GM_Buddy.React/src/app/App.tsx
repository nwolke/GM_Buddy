import { BrowserRouter, Routes, Route } from "react-router-dom";
import { AuthProvider } from "@/contexts/AuthContext";
import { Toaster } from "@/app/components/ui/sonner";
import { LandingPage } from "@/app/pages/LandingPage";
import { NPCManagerPage } from "@/app/pages/NPCManagerPage";
import { PCManagerPage } from "@/app/pages/PCManagerPage";
import { CampaignManagerPage } from "@/app/pages/CampaignManagerPage";
import { RelationshipsPage } from "@/app/pages/RelationshipsPage";
import { AccountPage } from "@/app/pages/AccountPage";
import { AboutPage } from "@/app/pages/AboutPage";
import { ProtectedRoute } from "@/app/components/ProtectedRoute";

console.log('[App] GM Buddy React App loading - v7');

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Toaster />
        <Routes>
          <Route path="/" element={<LandingPage />} />
          <Route 
            path="/npc-manager" 
            element={
              <ProtectedRoute>
                <NPCManagerPage />
              </ProtectedRoute>
            } 
          />
          <Route
            path="/pc-manager"
            element={
              <ProtectedRoute>
                <PCManagerPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/campaign-manager"
            element={<CampaignManagerPage />}
          />
          <Route
            path="/relationships"
            element={
              <ProtectedRoute>
                <RelationshipsPage />
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
          <Route path="/callback" element={<LandingPage />} />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  );
}