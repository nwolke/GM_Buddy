import { useState, useEffect } from "react";
import { NPC, Relationship } from "@/types/npc";
import { NPCCard } from "@/app/components/NPCCard";
import { NPCForm } from "@/app/components/NPCForm";
import { RelationshipManager } from "@/app/components/RelationshipManager";
import { NPCNetwork } from "@/app/components/NPCNetwork";
import { Button } from "@/app/components/ui/button";
import { Plus, Users, Scroll, RefreshCw, LogIn, LogOut, User } from "lucide-react";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/app/components/ui/tabs";
import { Shield } from "lucide-react";
import { AuthProvider, useAuth } from "@/contexts/AuthContext";
import { useNPCData } from "@/hooks/useNPCData";

console.log('[App] GM Buddy React App loading - v6');

function NPCApp() {
  const { isAuthenticated, user, login, loginWithCognito, logout, loading: authLoading, isCognitoMode } = useAuth();
  const {
    npcs,
    relationships,
    loading,
    error,
    refreshNpcs,
    saveNPC,
    deleteNPC,
    addRelationship,
    deleteRelationship,
  } = useNPCData();
  
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingNPC, setEditingNPC] = useState<NPC | null>(null);
  const [isRelationshipManagerOpen, setIsRelationshipManagerOpen] = useState(false);

  // Log component mount
  useEffect(() => {
    console.log('[NPCApp] Component mounted, authLoading:', authLoading, 'loading:', loading);
  }, []);
  const [currentNPC, setCurrentNPC] = useState<NPC | null>(null);

  const handleSaveNPC = async (npcData: Omit<NPC, 'id'> | NPC) => {
    await saveNPC(npcData);
    setEditingNPC(null);
  };

  const handleEditNPC = (npc: NPC) => {
    setEditingNPC(npc);
    setIsFormOpen(true);
  };

  const handleDeleteNPC = async (id: string) => {
    if (confirm('Are you sure you want to delete this NPC? This will also remove all their relationships.')) {
      await deleteNPC(id);
    }
  };

  const handleOpenRelationshipManager = (npc: NPC) => {
    setCurrentNPC(npc);
    setIsRelationshipManagerOpen(true);
  };

  const handleAddRelationship = (relationshipData: Omit<Relationship, 'id'>) => {
    addRelationship(relationshipData);
  };

  const handleDeleteRelationship = (id: string) => {
    deleteRelationship(id);
  };

  const getRelationshipCount = (npcId: string) => {
    return relationships.filter(
      rel => rel.npcId1 === npcId || rel.npcId2 === npcId
    ).length;
  };

  // Demo login handler - uses the seeded 'dev-user-sub' account from init.sql
  // In production, this would redirect to Cognito Hosted UI
  const handleDemoLogin = async (accountType: 'admin' | 'demo' = 'admin') => {
    // Clear any stale auth data first
    localStorage.removeItem('gm_buddy_auth');
    
    try {
      if (accountType === 'admin') {
        // Use the seeded gm_admin account (has sample NPCs)
        await login('dev-user-sub', 'gm_admin@example.local');
      } else {
        // Use the demo_user account
        await login('demo-user-sub', 'demo@example.com');
      }
    } catch (err) {
      console.error('Demo login failed:', err);
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
      {/* Decorative background pattern */}
      <div className="fixed inset-0 opacity-5 pointer-events-none">
        <div className="absolute inset-0" style={{
          backgroundImage: `radial-gradient(circle at 2px 2px, currentColor 1px, transparent 0)`,
          backgroundSize: '40px 40px'
        }} />
      </div>

      <div className="container mx-auto py-8 px-4 relative">
        {/* Header with auth controls */}
        <div className="flex items-center justify-between mb-8">
          <div className="flex items-center gap-4">
            <div className="bg-gradient-to-br from-primary to-accent p-3 rounded-xl shadow-lg shadow-primary/30">
              <Scroll className="size-8 text-primary-foreground" />
            </div>
            <div>
              <h1 className="text-5xl font-bold mb-2 bg-gradient-to-r from-primary via-accent to-primary bg-clip-text text-transparent">
                GM Buddy
              </h1>
              <p className="text-muted-foreground flex items-center gap-2">
                <span className="text-accent">⚔️</span>
                Manage your campaign's characters and their bonds
                <span className="text-primary">✨</span>
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
            <Button
              variant="ghost"
              size="icon"
              onClick={refreshNpcs}
              disabled={loading}
              title="Refresh NPCs"
            >
              <RefreshCw className={`size-4 ${loading ? 'animate-spin' : ''}`} />
            </Button>
            {isAuthenticated ? (
              <div className="flex items-center gap-2">
                <span className="text-sm text-muted-foreground">{user?.email}</span>
                <Button variant="outline" size="sm" onClick={logout}>
                  <LogOut className="size-4 mr-2" />
                  Logout
                </Button>
              </div>
            ) : (
              <div className="flex items-center gap-2">
                {isCognitoMode ? (
                  // Real Cognito login
                  <Button variant="default" size="sm" onClick={loginWithCognito}>
                    <LogIn className="size-4 mr-2" />
                    Sign In
                  </Button>
                ) : (
                  // Demo mode - show test account options
                  <>
                    <Button variant="default" size="sm" onClick={() => handleDemoLogin('admin')}>
                      <User className="size-4 mr-2" />
                      GM Admin
                    </Button>
                    <Button variant="outline" size="sm" onClick={() => handleDemoLogin('demo')}>
                      <User className="size-4 mr-2" />
                      Demo User
                    </Button>
                  </>
                )}
                <Button 
                  variant="ghost" 
                  size="sm" 
                  onClick={() => {
                    localStorage.clear();
                    window.location.reload();
                  }}
                  className="text-xs text-muted-foreground"
                  title="Clear cached data"
                >
                  Reset
                </Button>
              </div>
            )}
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
          </div>
        </div>

        <Tabs defaultValue="npcs" className="w-full">
          <TabsList className="mb-6 bg-card/50 border border-primary/20 p-1">
            <TabsTrigger 
              value="npcs"
              className="data-[state=active]:bg-gradient-to-r data-[state=active]:from-primary data-[state=active]:to-accent data-[state=active]:text-primary-foreground"
            >
              <Shield className="size-4 mr-2" />
              Characters ({npcs.length})
            </TabsTrigger>
            <TabsTrigger 
              value="network"
              className="data-[state=active]:bg-gradient-to-r data-[state=active]:from-primary data-[state=active]:to-accent data-[state=active]:text-primary-foreground"
            >
              <Users className="size-4 mr-2" />
              Relationship Web
            </TabsTrigger>
          </TabsList>

          <TabsContent value="npcs">
            {loading ? (
              <div className="text-center py-20">
                <RefreshCw className="size-12 animate-spin text-primary mx-auto mb-4" />
                <p className="text-muted-foreground">Loading NPCs...</p>
              </div>
            ) : npcs.length === 0 ? (
              <div className="text-center py-20">
                <div className="bg-gradient-to-br from-card to-secondary/30 border border-primary/30 rounded-2xl p-12 max-w-md mx-auto shadow-xl">
                  <div className="bg-gradient-to-br from-primary/20 to-accent/20 w-24 h-24 rounded-full flex items-center justify-center mx-auto mb-6">
                    <Users className="size-12 text-primary" />
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
                    onManageRelationships={handleOpenRelationshipManager}
                    relationshipCount={getRelationshipCount(npc.id)}
                  />
                ))}
              </div>
            )}
          </TabsContent>

          <TabsContent value="network">
            <NPCNetwork
              npcs={npcs}
              relationships={relationships}
              onNodeClick={handleOpenRelationshipManager}
            />
          </TabsContent>
        </Tabs>
      </div>

      <NPCForm
        open={isFormOpen}
        onOpenChange={setIsFormOpen}
        onSave={handleSaveNPC}
        editingNPC={editingNPC}
      />

      <RelationshipManager
        open={isRelationshipManagerOpen}
        onOpenChange={setIsRelationshipManagerOpen}
        currentNPC={currentNPC}
        allNPCs={npcs}
        relationships={relationships}
        onAddRelationship={handleAddRelationship}
        onDeleteRelationship={handleDeleteRelationship}
      />
    </div>
  );
}

// Wrap the app with AuthProvider
export default function App() {
  return (
    <AuthProvider>
      <NPCApp />
    </AuthProvider>
  );
}