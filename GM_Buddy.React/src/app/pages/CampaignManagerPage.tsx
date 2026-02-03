import { useState } from "react";
import { Campaign } from "@/types/campaign";
import { CampaignCard } from "@/app/components/CampaignCard";
import { CampaignForm } from "@/app/components/CampaignForm";
import { Button } from "@/app/components/ui/button";
import { Plus, RefreshCw, LogIn } from "lucide-react";
import { useAuth } from "@/contexts/AuthContext";
import { useCampaignData } from "@/hooks/useCampaignData";
import { Header } from "@/app/components/Header";

export function CampaignManagerPage() {
  const { isAuthenticated, loginWithCognito, loading: authLoading } = useAuth();
  const {
    campaigns,
    loading,
    error,
    refreshCampaigns,
    saveCampaign,
    deleteCampaign,
  } = useCampaignData();

  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingCampaign, setEditingCampaign] = useState<Campaign | null>(null);

  const handleSaveCampaign = async (campaignData: Omit<Campaign, 'id'> | Campaign) => {
    await saveCampaign(campaignData);
    setEditingCampaign(null);
  };

  const handleEditCampaign = (campaign: Campaign) => {
    setEditingCampaign(campaign);
    setIsFormOpen(true);
  };

  const handleDeleteCampaign = async (id: number) => {
    if (confirm('Are you sure you want to delete this campaign?')) {
      await deleteCampaign(id);
    }
  };

  if (authLoading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-background via-background to-secondary/20 flex items-center justify-center">
        <div className="text-center">
          <RefreshCw className="size-8 animate-spin text-primary mx-auto mb-4" />
          <p className="text-muted-foreground">Loading...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-background via-background to-secondary/20">
      <div className="fixed inset-0 opacity-5 pointer-events-none">
        <div className="absolute inset-0" style={{
          backgroundImage: `radial-gradient(circle at 2px 2px, currentColor 1px, transparent 0)`,
          backgroundSize: '40px 40px'
        }} />
      </div>

      <div className="container mx-auto py-8 px-4 relative">
        <Header 
          showRefresh={true}
          onRefresh={refreshCampaigns}
          loading={loading}
          error={error}
        />

        <div className="flex items-center justify-between mb-6">
          <div>
            <h2 className="text-3xl font-bold">Campaigns</h2>
            <p className="text-muted-foreground mt-2">
              Manage your tabletop campaigns
            </p>
          </div>
          {isAuthenticated && (
            <Button 
              onClick={() => {
                setEditingCampaign(null);
                setIsFormOpen(true);
              }} 
              size="lg"
              className="bg-gradient-to-r from-primary to-accent hover:from-primary/90 hover:to-accent/90 shadow-lg shadow-primary/30"
            >
              <Plus className="size-4 mr-2" />
              New Campaign
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
                Sign in to manage your campaigns
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
            <p className="text-muted-foreground">Loading campaigns...</p>
          </div>
        ) : campaigns.length === 0 ? (
          <div className="text-center py-20">
            <div className="bg-gradient-to-br from-card to-secondary/30 border border-primary/30 rounded-2xl p-12 max-w-md mx-auto shadow-xl">
              <div className="bg-gradient-to-br from-primary/20 to-accent/20 w-24 h-24 rounded-full flex items-center justify-center mx-auto mb-6">
                <Plus className="size-12 text-primary" />
              </div>
              <h3 className="text-2xl font-bold mb-3 text-primary">No Campaigns Yet</h3>
              <p className="text-muted-foreground mb-6 leading-relaxed">
                Create your first campaign to get started
              </p>
              <Button 
                onClick={() => {
                  setEditingCampaign(null);
                  setIsFormOpen(true);
                }}
                size="lg"
                className="bg-gradient-to-r from-primary to-accent hover:from-primary/90 hover:to-accent/90 shadow-lg"
              >
                <Plus className="size-4 mr-2" />
                Create First Campaign
              </Button>
            </div>
          </div>
        ) : (
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            {campaigns.map(campaign => (
              <CampaignCard
                key={campaign.id}
                campaign={campaign}
                onEdit={handleEditCampaign}
                onDelete={handleDeleteCampaign}
              />
            ))}
          </div>
        )}
      </div>

      <CampaignForm
        open={isFormOpen}
        onOpenChange={setIsFormOpen}
        onSave={handleSaveCampaign}
        editingCampaign={editingCampaign}
      />
    </div>
  );
}
