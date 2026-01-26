import { Scroll, RefreshCw, LogIn, LogOut } from "lucide-react";
import { Button } from "@/app/components/ui/button";
import { useAuth } from "@/contexts/AuthContext";

interface HeaderProps {
  showRefresh?: boolean;
  onRefresh?: () => void;
  loading?: boolean;
  error?: string | null;
}

export function Header({ showRefresh = false, onRefresh, loading = false, error = null }: HeaderProps) {
  const { isAuthenticated, user, loginWithCognito, logout } = useAuth();

  return (
    <div className="flex items-center justify-between mb-8">
      <div className="flex items-center gap-4">
        <div className="bg-gradient-to-br from-primary to-accent p-3 rounded-xl shadow-lg shadow-primary/30">
          <Scroll className="size-8 text-primary-foreground" />
        </div>
        <div>
          <h1 className="text-5xl font-bold mb-2 bg-gradient-to-r from-primary via-accent to-primary bg-clip-text text-transparent">
            GM Buddy
          </h1>
          <p className="text-muted-foreground">
            Manage your campaign's characters and their bonds
          </p>
        </div>
      </div>
      <div className="flex items-center gap-3">
        {error && (
          <span className="text-sm text-destructive">{error}</span>
        )}
        {loading && (
          <RefreshCw className="size-4 animate-spin text-muted-foreground" />
        )}
        {showRefresh && onRefresh && (
          <Button
            variant="ghost"
            size="icon"
            onClick={onRefresh}
            disabled={loading}
            title="Refresh NPCs"
          >
            <RefreshCw className={`size-4 ${loading ? 'animate-spin' : ''}`} />
          </Button>
        )}
        {isAuthenticated ? (
          <div className="flex items-center gap-2">
            <span className="text-sm text-muted-foreground">{user?.email}</span>
            <Button variant="outline" size="sm" onClick={logout}>
              <LogOut className="size-4 mr-2" />
              Logout
            </Button>
          </div>
        ) : (
          <Button variant="default" size="sm" onClick={loginWithCognito}>
            <LogIn className="size-4 mr-2" />
            Sign In
          </Button>
        )}
      </div>
    </div>
  );
}
