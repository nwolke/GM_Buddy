import { useState } from "react";
import { PC } from "@/types/pc";
import { PCCard } from "@/app/components/PCCard";
import { PCForm } from "@/app/components/PCForm";
import { Button } from "@/app/components/ui/button";
import { Plus, RefreshCw, LogIn, User, Filter } from "lucide-react";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/app/components/ui/select";
import { useAuth } from "@/contexts/AuthContext";
import { usePCData } from "@/hooks/usePCData";
import { useCampaignData } from "@/hooks/useCampaignData";
import { Header } from "@/app/components/Header";

export function PCManagerPage() {
  const { isAuthenticated, loginWithCognito } = useAuth();
  const { campaigns, loading: campaignsLoading } = useCampaignData();
  const [selectedCampaignId, setSelectedCampaignId] = useState<number | undefined>(undefined);
  const { pcs, loading, error, refreshPcs, savePc, deletePc } = usePCData(selectedCampaignId);

  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingPC, setEditingPC] = useState<PC | null>(null);

  const handleSavePC = async (pcData: Omit<PC, 'id'> | PC) => {
    await savePc(pcData);
    setEditingPC(null);
  };

  const handleEditPC = (pc: PC) => {
    setEditingPC(pc);
    setIsFormOpen(true);
  };

  const handleDeletePC = async (id: number) => {
    if (confirm('Are you sure you want to delete this player character?')) {
      await deletePc(id);
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-background via-background to-secondary/20">
      {/* Decorative background pattern */}
      <div className="fixed inset-0 opacity-5 pointer-events-none">
        <div className="absolute inset-0" style={{
          backgroundImage: `radial-gradient(circle at 2px 2px, currentColor 1px, transparent 0)`,
          backgroundSize: '40px 40px'
        }} />
      </div>

      <div className="container mx-auto py-8 px-4 relative">
        <Header
          showRefresh={true}
          onRefresh={refreshPcs}
          loading={loading}
          error={error}
        />

        <div className="flex items-center justify-between mb-6 gap-4">
          <div className="flex items-center gap-4">
            <div className="flex items-center gap-2 bg-card/50 border border-primary/20 rounded-lg px-4 py-2">
              <User className="size-4 text-primary" />
              <span className="text-sm font-medium">
                Player Characters ({pcs.length})
              </span>
            </div>

            {isAuthenticated && !campaignsLoading && campaigns.length > 0 && (
              <Select
                value={selectedCampaignId?.toString() ?? "all"}
                onValueChange={(value) => {
                  if (value === "all") {
                    setSelectedCampaignId(undefined);
                    return;
                  }
                  const numericId = Number(value);
                  setSelectedCampaignId(Number.isNaN(numericId) ? undefined : numericId);
                }}
              >
                <SelectTrigger className="w-[250px] bg-card/50 border-primary/20">
                  <Filter className="size-4 mr-2" />
                  <SelectValue placeholder="All Campaigns" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Campaigns</SelectItem>
                  {campaigns.map((campaign) => (
                    <SelectItem key={campaign.id} value={campaign.id.toString()}>
                      {campaign.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            )}
          </div>

          {isAuthenticated && (
            <Button
              onClick={() => {
                setEditingPC(null);
                setIsFormOpen(true);
              }}
              size="lg"
              className="bg-gradient-to-r from-primary to-accent hover:from-primary/90 hover:to-accent/90 shadow-lg shadow-primary/30"
            >
              <Plus className="size-4 mr-2" />
              Add Character
            </Button>
          )}
        </div>

        {!isAuthenticated ? (
          <div className="text-center py-20">
            <div className="bg-gradient-to-br from-card to-secondary/30 border border-primary/30 rounded-2xl p-12 max-w-md mx-auto shadow-xl">
              <div className="bg-gradient-to-br from-primary/20 to-accent/20 w-24 h-24 rounded-full flex items-center justify-center mx-auto mb-6">
                <LogIn className="size-12 text-primary" />
              </div>
              <h3 className="text-2xl font-bold mb-3 text-primary">Sign In Required</h3>
              <p className="text-muted-foreground mb-6 leading-relaxed">
                Sign in to manage your player characters
              </p>
              <Button
                onClick={loginWithCognito}
                size="lg"
                className="bg-gradient-to-r from-primary to-accent hover:from-primary/90 hover:to-accent/90 shadow-lg"
              >
                <LogIn className="size-4 mr-2" />
                Sign In with Cognito
              </Button>
            </div>
          </div>
        ) : loading ? (
          <div className="text-center py-20">
            <RefreshCw className="size-12 animate-spin text-primary mx-auto mb-4" />
            <p className="text-muted-foreground">Loading player characters...</p>
          </div>
        ) : pcs.length === 0 ? (
          <div className="text-center py-20">
            <div className="bg-gradient-to-br from-card to-secondary/30 border border-primary/30 rounded-2xl p-12 max-w-md mx-auto shadow-xl">
              <div className="bg-gradient-to-br from-primary/20 to-accent/20 w-24 h-24 rounded-full flex items-center justify-center mx-auto mb-6">
                <User className="size-12 text-primary" />
              </div>
              <h3 className="text-2xl font-bold mb-3 text-primary">No Characters Yet</h3>
              <p className="text-muted-foreground mb-6 leading-relaxed">
                {selectedCampaignId
                  ? "No player characters found in this campaign."
                  : "Add your first player character to get started."}
              </p>
              {!selectedCampaignId && (
                <Button
                  onClick={() => {
                    setEditingPC(null);
                    setIsFormOpen(true);
                  }}
                  size="lg"
                  className="bg-gradient-to-r from-primary to-accent hover:from-primary/90 hover:to-accent/90 shadow-lg"
                >
                  <Plus className="size-4 mr-2" />
                  Add First Character
                </Button>
              )}
            </div>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {pcs.map(pc => (
              <PCCard
                key={pc.id}
                pc={pc}
                onEdit={handleEditPC}
                onDelete={handleDeletePC}
              />
            ))}
          </div>
        )}
      </div>

      <PCForm
        open={isFormOpen}
        onOpenChange={setIsFormOpen}
        onSave={handleSavePC}
        editingPC={editingPC}
      />
    </div>
  );
}
