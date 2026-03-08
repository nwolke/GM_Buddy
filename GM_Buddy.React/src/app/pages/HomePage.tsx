import { Link } from "react-router-dom";
import { Network, Lock, ArrowRight, LogIn } from "lucide-react";
import { Button } from "@/app/components/ui/button";
import { Card, CardContent } from "@/app/components/ui/card";
import { useAuth } from "@/contexts/AuthContext";
import { Header } from "@/app/components/Header";

interface ToolCardProps {
  title: string;
  description: string;
  icon: React.ReactNode;
  gradientFrom: string;
  gradientTo: string;
  to?: string;
  disabled?: boolean;
}

function ToolCardContent({ title, description, icon, gradientFrom, gradientTo, disabled }: Omit<ToolCardProps, 'to'>) {
  return (
    <Card
      className={`group transition-all duration-300 border-primary/30 overflow-hidden ${
        disabled
          ? 'opacity-50 cursor-not-allowed'
          : 'hover:shadow-xl hover:shadow-primary/20 hover:border-primary/50 hover:scale-[1.02]'
      }`}
      aria-disabled={disabled || undefined}
    >
      {/* Placeholder image area with gradient + icon */}
      <div
        className="h-48 flex items-center justify-center relative overflow-hidden"
        style={{
          background: `linear-gradient(135deg, ${gradientFrom}, ${gradientTo})`,
        }}
      >
        <div className="absolute inset-0 opacity-10">
          <div
            className="absolute inset-0"
            style={{
              backgroundImage: `radial-gradient(circle at 2px 2px, currentColor 1px, transparent 0)`,
              backgroundSize: '24px 24px',
            }}
          />
        </div>
        <div className="relative z-10 bg-white/20 backdrop-blur-sm p-6 rounded-2xl shadow-lg">
          {disabled ? (
            <Lock className="size-16 text-white/80" />
          ) : (
            icon
          )}
        </div>
      </div>

      <CardContent className="p-6">
        <h3 className="text-xl font-bold mb-2 bg-gradient-to-r from-primary to-accent bg-clip-text text-transparent">
          {title}
        </h3>
        <p className="text-muted-foreground text-sm leading-relaxed mb-4">
          {description}
        </p>
        {!disabled && (
          <div className="flex items-center text-primary text-sm font-medium group-hover:translate-x-1 transition-transform">
            Open tool
            <ArrowRight className="size-4 ml-1" />
          </div>
        )}
        {disabled && (
          <span className="text-xs text-muted-foreground italic">Coming soon</span>
        )}
      </CardContent>
    </Card>
  );
}

function ToolCard(props: ToolCardProps) {
  const { to, disabled, ...cardProps } = props;

  if (to && !disabled) {
    return (
      <Link to={to} className="block no-underline rounded-2xl focus-visible:outline-2 focus-visible:outline-primary focus-visible:outline-offset-2">
        <ToolCardContent {...cardProps} disabled={false} />
      </Link>
    );
  }

  return <ToolCardContent {...cardProps} disabled={disabled} />;
}

export function HomePage() {
  const { isAuthenticated, loginWithCognito } = useAuth();

  return (
    <div className="min-h-screen bg-gradient-to-br from-background via-background to-secondary/20">
      {/* Decorative background */}
      <div className="fixed inset-0 opacity-5 pointer-events-none">
        <div
          className="absolute inset-0"
          style={{
            backgroundImage: `radial-gradient(circle at 2px 2px, currentColor 1px, transparent 0)`,
            backgroundSize: '40px 40px',
          }}
        />
      </div>

      <div className="container mx-auto py-8 px-4 relative">
        <Header />

        <div className="text-center mb-12 mt-8">
          <h2 className="text-4xl font-bold mb-4 bg-gradient-to-r from-primary via-accent to-primary bg-clip-text text-transparent">
            Welcome to GM Buddy
          </h2>
          <p className="text-lg text-muted-foreground max-w-xl mx-auto leading-relaxed">
            Your ultimate companion for managing campaigns, tracking characters, and bringing your world to life.
          </p>
        </div>

        {!isAuthenticated ? (
          <div className="text-center py-12">
            <div className="bg-gradient-to-br from-card to-secondary/30 border border-primary/30 rounded-2xl p-12 max-w-md mx-auto shadow-xl">
              <div className="bg-gradient-to-br from-primary/20 to-accent/20 w-24 h-24 rounded-full flex items-center justify-center mx-auto mb-6">
                <LogIn className="size-12 text-primary" />
              </div>
              <h3 className="text-2xl font-bold mb-3 text-primary">Sign In Required</h3>
              <p className="text-muted-foreground mb-6 leading-relaxed">
                Sign in to access your GM tools
              </p>
              <Button
                onClick={loginWithCognito}
                size="lg"
                className="bg-gradient-to-r from-primary to-accent hover:from-primary/90 hover:to-accent/90 shadow-lg"
              >
                <LogIn className="size-4 mr-2" />
                Sign In
              </Button>
            </div>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8 max-w-4xl mx-auto">
            <ToolCard
              title="Relationship Manager"
              description="Track your NPCs, player characters, and their connections across campaigns. Visualize relationships with an interactive graph."
              icon={<Network className="size-16 text-white" />}
              gradientFrom="hsl(270, 70%, 45%)"
              gradientTo="hsl(330, 70%, 50%)"
              to="/relationship-manager"
            />

            <ToolCard
              title="More Tools Coming"
              description="Additional GM tools are on the way. Stay tuned for encounter builders, world maps, and more."
              icon={<Lock className="size-16 text-white/80" />}
              gradientFrom="hsl(220, 30%, 30%)"
              gradientTo="hsl(250, 30%, 35%)"
              disabled
            />
          </div>
        )}
      </div>
    </div>
  );
}
