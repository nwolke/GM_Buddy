import { BrowserRouter, Routes, Route } from "react-router-dom";
import { AuthProvider } from "@/contexts/AuthContext";
import { LandingPage } from "@/app/pages/LandingPage";
import { NPCManagerPage } from "@/app/pages/NPCManagerPage";
import { ProtectedRoute } from "@/app/components/ProtectedRoute";

console.log('[App] GM Buddy React App loading - v7');

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
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
          <Route path="/callback" element={<LandingPage />} />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  );
}