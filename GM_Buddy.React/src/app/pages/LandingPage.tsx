import { useNavigate } from "react-router-dom";
import { Button } from "@/app/components/ui/button";
import { Users, ArrowRight, Scroll } from "lucide-react";
import { useAuth } from "@/contexts/AuthContext";
import { Header } from "@/app/components/Header";

export function LandingPage() {
  const navigate = useNavigate();
  const { isAuthenticated } = useAuth();

  const handleNavigateToNPCManager = () => {
    if (isAuthenticated) {
      navigate("/npc-manager");
    }
  };

  const handleNavigateToCampaignManager = () => {
    if (isAuthenticated) {
      navigate("/campaign-manager");
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-background via-background to-secondary/20">
      {/* Decorative background pattern */}
      <div className="fixed inset-0 opacity-5 pointer-events-none">
        <div
          className="absolute inset-0"
          style={{
            backgroundImage: `radial-gradient(circle at 2px 2px, currentColor 1px, transparent 0)`,
            backgroundSize: "40px 40px",
          }}
        />
      </div>

      <div className="container mx-auto py-8 px-4 relative">
        <Header />

        {/* Content */}
        <div className="flex items-center justify-center min-h-[calc(100vh-12rem)]">
          <div className="text-center max-w-2xl">
            {/* Icon */}
            <div className="bg-gradient-to-br from-primary to-accent p-6 rounded-2xl shadow-2xl shadow-primary/30 mx-auto mb-8 w-fit">
              <Users className="size-16 text-primary-foreground" />
            </div>

            {/* Heading */}
            <h2 className="text-4xl font-bold mb-4 bg-gradient-to-r from-primary via-accent to-primary bg-clip-text text-transparent">
              Welcome to GM Buddy
            </h2>

            {/* Description */}
            <p className="text-lg text-muted-foreground mb-8 leading-relaxed">
              Your ultimate companion for managing campaign NPCs, tracking relationships, and bringing your world to life.
            </p>

            {/* Action Button */}
            {isAuthenticated ? (
              <div className="flex gap-4 justify-center">
                <Button
                  size="lg"
                  onClick={handleNavigateToNPCManager}
                  className="bg-gradient-to-r from-primary to-accent hover:from-primary/90 hover:to-accent/90 shadow-lg shadow-primary/30 text-lg px-8 py-6"
                >
                  <Users className="size-5 mr-2" />
                  Manage NPCs
                  <ArrowRight className="size-5 ml-2" />
                </Button>
                <Button
                  size="lg"
                  onClick={handleNavigateToCampaignManager}
                  className="bg-gradient-to-r from-primary to-accent hover:from-primary/90 hover:to-accent/90 shadow-lg shadow-primary/30 text-lg px-8 py-6"
                >
                  <Scroll className="size-5 mr-2" />
                  Manage Campaigns
                  <ArrowRight className="size-5 ml-2" />
                </Button>
              </div>
            ) : (
              <div className="bg-card/50 border border-primary/20 rounded-xl p-8 max-w-md mx-auto">
                <p className="text-muted-foreground mb-4">
                  Please sign in to manage your NPCs
                </p>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
