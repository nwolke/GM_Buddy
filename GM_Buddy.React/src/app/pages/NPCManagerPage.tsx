import { useState, useEffect } from "react";
import { NPC } from "@/types/npc";
import { NPCCard } from "@/app/components/NPCCard";
import { NPCForm } from "@/app/components/NPCForm";
import { Button } from "@/app/components/ui/button";
import { Plus, RefreshCw, LogIn, Shield, Filter } from "lucide-react";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/app/components/ui/tabs";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/app/components/ui/select";
import { useAuth } from "@/contexts/AuthContext";
import { useNPCData } from "@/hooks/useNPCData";
import { useCampaignData } from "@/hooks/useCampaignData";
import { Header } from "@/app/components/Header";

export function NPCManagerPage() {
const { isAuthenticated, loginWithCognito, loading: authLoading } = useAuth();
const { campaigns, loading: campaignsLoading } = useCampaignData();
const [selectedCampaignId, setSelectedCampaignId] = useState<number | undefined>(undefined);
const {
  npcs,
  relationships,
  loading,
  error,
  refreshNpcs,
  saveNPC,
  deleteNPC,
} = useNPCData(selectedCampaignId);

  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingNPC, setEditingNPC] = useState<NPC | null>(null);

  useEffect(() => {
    console.log('[NPCManagerPage] Auth/loading state changed or component mounted. authLoading:', authLoading, 'loading:', loading);
  }, [authLoading, loading]);

  const handleSaveNPC = async (npcData: Omit<NPC, 'id'> | NPC) => {
    await saveNPC(npcData);
    setEditingNPC(null);
  };

  const handleEditNPC = (npc: NPC) => {
    setEditingNPC(npc);
    setIsFormOpen(true);
  };

  const handleDeleteNPC = async (id: number) => {
    if (confirm('Are you sure you want to delete this NPC? This will also remove all their relationships.')) {
      await deleteNPC(id);
    }
  };

  const getRelationshipCount = (npcId: number) => {
    return relationships.filter(
      rel => rel.npcId1 === npcId || rel.npcId2 === npcId
    ).length;
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
          onRefresh={refreshNpcs}
          loading={loading}
          error={error}
        />

        <Tabs defaultValue="npcs" className="w-full">
          <div className="flex items-center justify-between mb-6 gap-4">
            <div className="flex items-center gap-4">
              <TabsList className="bg-card/50 border border-primary/20 p-1">
                <TabsTrigger
                  value="npcs"
                  className="data-[state=active]:bg-gradient-to-r data-[state=active]:from-primary data-[state=active]:to-accent data-[state=active]:text-primary-foreground"
                >
                  <Shield className="size-4 mr-2" />
                  Characters ({npcs.length})
                </TabsTrigger>
              </TabsList>

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
                  setEditingNPC(null);
                  setIsFormOpen(true);
                }}
                size="lg"
                className="bg-gradient-to-r from-primary to-accent hover:from-primary/90 hover:to-accent/90 shadow-lg shadow-primary/30"
              >
                <Plus className="size-4 mr-2" />
                Recruit NPC
              </Button>
            )}
          </div>

          <TabsContent value="npcs">
            {!isAuthenticated ? (
              <div className="text-center py-20">
                <div className="bg-gradient-to-br from-card to-secondary/30 border border-primary/30 rounded-2xl p-12 max-w-md mx-auto shadow-xl">
                  <div className="bg-gradient-to-br from-primary/20 to-accent/20 w-24 h-24 rounded-full flex items-center justify-center mx-auto mb-6">
                    <LogIn className="size-12 text-primary" />
                  </div>
                  <h3 className="text-2xl font-bold mb-3 text-primary">Sign In Required</h3>
                  <p className="text-muted-foreground mb-6 leading-relaxed">
                    Sign in to manage your campaign's characters and relationships
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
                <p className="text-muted-foreground">Loading NPCs...</p>
              </div>
            ) : npcs.length === 0 ? (
              <div className="text-center py-20">
                <div className="bg-gradient-to-br from-card to-secondary/30 border border-primary/30 rounded-2xl p-12 max-w-md mx-auto shadow-xl">
                  <div className="bg-gradient-to-br from-primary/20 to-accent/20 w-24 h-24 rounded-full flex items-center justify-center mx-auto mb-6">
                    <Shield className="size-12 text-primary" />
                  </div>
                  <h3 className="text-2xl font-bold mb-3 text-primary">No Characters Yet</h3>
                  <p className="text-muted-foreground mb-6 leading-relaxed">
                    Begin your epic tale by recruiting your first NPC to the compendium
                  </p>
                  <Button
                    onClick={() => {
                      setEditingNPC(null);
                      setIsFormOpen(true);
                    }}
                    size="lg"
                    className="bg-gradient-to-r from-primary to-accent hover:from-primary/90 hover:to-accent/90 shadow-lg"
                  >
                    <Plus className="size-4 mr-2" />
                    Recruit First Character
                  </Button>
                </div>
              </div>
            ) : (
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                {npcs.map(npc => (
                  <NPCCard
                    key={npc.id}
                    npc={npc}
                    onEdit={handleEditNPC}
                    onDelete={handleDeleteNPC}
                    relationshipCount={getRelationshipCount(npc.id)}
                  />
                ))}
              </div>
            )}
          </TabsContent>
        </Tabs>
      </div>

      <NPCForm
        open={isFormOpen}
        onOpenChange={setIsFormOpen}
        onSave={handleSaveNPC}
        editingNPC={editingNPC}
      />
    </div>
  );
}
