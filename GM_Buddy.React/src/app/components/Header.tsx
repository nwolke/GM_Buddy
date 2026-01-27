import { Scroll, RefreshCw, LogIn, LogOut, Users } from "lucide-react";
import { Button } from "@/app/components/ui/button";
import { useAuth } from "@/contexts/AuthContext";
import { useNavigate, useLocation, Link } from "react-router-dom";

interface HeaderProps {
  showRefresh?: boolean;
  onRefresh?: () => void;
  loading?: boolean;
  error?: string | null;
}

export function Header({ showRefresh = false, onRefresh, loading = false, error = null }: HeaderProps) {
  const { isAuthenticated, user, loginWithCognito, logout } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const isOnNPCManager = location.pathname === '/npc-manager';
  const isOnCampaignManager = location.pathname === '/campaign-manager';

  return (
    <div className="flex items-center justify-between mb-8">
      <div className="flex items-center gap-4">
        <Link 
          to="/"
          className="bg-gradient-to-br from-primary to-accent p-3 rounded-xl shadow-lg shadow-primary/30 cursor-pointer inline-block"
        >
          <Scroll className="size-8 text-primary-foreground" />
        </Link>
        <div>
          <Link to="/">
            <h1 
              className="text-5xl font-bold mb-2 bg-gradient-to-r from-primary via-accent to-primary bg-clip-text text-transparent cursor-pointer"
            >
              GM Buddy
            </h1>
          </Link>
          <p className="text-muted-foreground flex items-center gap-2">
            <span className="text-accent" aria-hidden="true">?</span>
            Manage your campaign's characters and their bonds
            <span className="text-primary" aria-hidden="true">?</span>
          </p>
        </div>
      </div>
      <div className="flex items-center gap-3">
        {isAuthenticated && (isOnNPCManager || isOnCampaignManager) && (
          <div className="flex gap-2 mr-4">
            <Button
              variant={isOnNPCManager ? "default" : "outline"}
              size="sm"
              onClick={() => navigate('/npc-manager')}
            >
              <Users className="size-4 mr-2" />
              NPCs
            </Button>
            <Button
              variant={isOnCampaignManager ? "default" : "outline"}
              size="sm"
              onClick={() => navigate('/campaign-manager')}
            >
              <Scroll className="size-4 mr-2" />
              Campaigns
            </Button>
          </div>
        )}
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
            title="Refresh"
            aria-label="Refresh"
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
