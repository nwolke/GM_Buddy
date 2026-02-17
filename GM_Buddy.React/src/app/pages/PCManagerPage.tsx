import { useState } from "react";
import { PC } from "@/types/pc";
import { PCCard } from "@/app/components/PCCard";
import { PCForm } from "@/app/components/PCForm";
import { Button } from "@/app/components/ui/button";
import { Plus, RefreshCw, LogIn, User } from "lucide-react";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/app/components/ui/tabs";
import { useAuth } from "@/contexts/AuthContext";
import { usePCData } from "@/hooks/usePCData";
import { Header } from "@/app/components/Header";

export function PCManagerPage() {
  const { isAuthenticated, loginWithCognito, loading: authLoading } = useAuth();
  const {
    pcs,
    loading,
    error,
    refreshPcs,
    savePC,
    deletePC,
  } = usePCData();

  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingPC, setEditingPC] = useState<PC | null>(null);

  const handleSavePC = async (pcData: Omit<PC, 'id'> | PC) => {
    await savePC(pcData);
    setEditingPC(null);
  };

  const handleEditPC = (pc: PC) => {
    setEditingPC(pc);
    setIsFormOpen(true);
  };

  const handleDeletePC = async (id: number) => {
    if (confirm('Are you sure you want to delete this player character?')) {
      await deletePC(id);
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
        <Header 
          showRefresh={true}
          onRefresh={refreshPcs}
          loading={loading}
          error={error}
        />

        <Tabs defaultValue="pcs" className="w-full">
          <div className="flex items-center justify-between mb-6 gap-4">
            <div className="flex items-center gap-4">
              <TabsList className="bg-card/50 border border-primary/20 p-1">
                <TabsTrigger 
                  value="pcs"
                  className="data-[state=active]:bg-gradient-to-r data-[state=active]:from-primary data-[state=active]:to-accent data-[state=active]:text-primary-foreground"
                >
                  <User className="size-4 mr-2" />
                  Player Characters ({pcs.length})
                </TabsTrigger>
              </TabsList>
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

          <TabsContent value="pcs">
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
                    Begin your adventure by creating your first player character
                  </p>
                  <Button 
                    onClick={() => {
                      setEditingPC(null);
                      setIsFormOpen(true);
                    }}
                    size="lg"
                    className="bg-gradient-to-r from-primary to-accent hover:from-primary/90 hover:to-accent/90 shadow-lg"
                  >
                    <Plus className="size-4 mr-2" />
                    Create First Character
                  </Button>
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
          </TabsContent>
        </Tabs>
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
