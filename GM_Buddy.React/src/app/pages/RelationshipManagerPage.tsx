import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Campaign } from "@/types/campaign";
import { CampaignForm } from "@/app/components/CampaignForm";
import { Button } from "@/app/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/app/components/ui/card";
import { Plus, RefreshCw, LogIn, BookOpen, Pencil, Trash2, ArrowRight } from "lucide-react";
import { useAuth } from "@/contexts/AuthContext";
import { useCampaignData } from "@/hooks/useCampaignData";
import { Header } from "@/app/components/Header";

function truncateDescription(text: string | undefined, maxLength: number = 500): string {
  if (!text) return "";
  if (text.length <= maxLength) return text;
  return text.slice(0, maxLength).trimEnd() + "...";
}

export function RelationshipManagerPage() {
  const navigate = useNavigate();
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

  const handleEditCampaign = (e: React.MouseEvent, campaign: Campaign) => {
    e.stopPropagation();
    setEditingCampaign(campaign);
    setIsFormOpen(true);
  };

  const handleDeleteCampaign = async (e: React.MouseEvent, id: number) => {
    e.stopPropagation();
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
          breadcrumbs={[
            { label: "Relationship Manager" },
          ]}
        />

        <div className="flex items-center justify-between mb-6">
          <div>
            <h2 className="text-3xl font-bold bg-gradient-to-r from-primary to-accent bg-clip-text text-transparent">
              Campaigns
            </h2>
            <p className="text-muted-foreground mt-2">
              Select a campaign to manage its characters and relationships
            </p>
          </div>
          <div className="flex items-center gap-3">
            {loading && (
              <RefreshCw className="size-4 animate-spin text-muted-foreground" />
            )}
            {error && (
              <span className="text-sm text-destructive">{error}</span>
            )}
            {isAuthenticated && (
              <>
                <Button
                  variant="ghost"
                  size="icon"
                  onClick={refreshCampaigns}
                  disabled={loading}
                  title="Refresh"
                  aria-label="Refresh campaigns"
                >
                  <RefreshCw className={`size-4 ${loading ? 'animate-spin' : ''}`} />
                </Button>
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
              </>
            )}
          </div>
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
                Sign In
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
                <BookOpen className="size-12 text-primary" />
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
              <Card
                key={campaign.id}
                className="group hover:shadow-xl transition-all duration-300 bg-gradient-to-br from-card to-secondary/30 border-primary/30 hover:border-primary/50 cursor-pointer hover:scale-[1.01]"
                onClick={() => navigate(`/campaign/${campaign.id}`)}
              >
                <CardHeader className="pb-3">
                  <div className="flex items-start justify-between gap-4">
                    <CardTitle className="text-2xl bg-gradient-to-r from-primary to-accent bg-clip-text text-transparent">
                      {campaign.name}
                    </CardTitle>
                    <ArrowRight className="size-5 text-muted-foreground group-hover:text-primary group-hover:translate-x-1 transition-all shrink-0 mt-1" />
                  </div>
                </CardHeader>
                <CardContent>
                  {campaign.description && (
                    <p className="text-muted-foreground text-sm mb-4 leading-relaxed">
                      {truncateDescription(campaign.description)}
                    </p>
                  )}

                  <div className="flex gap-2 mt-4 pt-4 border-t border-primary/20">
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={(e) => handleEditCampaign(e, campaign)}
                      className="flex-1 hover:bg-primary/10 hover:border-primary/50"
                    >
                      <Pencil className="size-4 mr-2" />
                      Edit
                    </Button>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={(e) => handleDeleteCampaign(e, campaign.id)}
                      className="hover:bg-destructive/10 hover:border-destructive/50 hover:text-destructive"
                      aria-label="Delete campaign"
                      title="Delete campaign"
                    >
                      <Trash2 className="size-4" />
                    </Button>
                  </div>
                </CardContent>
              </Card>
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
