import { useState, useRef, useEffect, useMemo } from "react";
import { useParams, Navigate } from "react-router-dom";
import { NPC } from "@/types/npc";
import { PC } from "@/types/pc";
import { EntityItem } from "@/types/entity";
import { useNPCData } from "@/hooks/useNPCData";
import { usePCData } from "@/hooks/usePCData";
import { useCampaignData } from "@/hooks/useCampaignData";
import { EntityGraph } from "@/app/components/EntityGraph";
import { EntityDetailPanel } from "@/app/components/EntityDetailPanel";
import { NPCForm } from "@/app/components/NPCForm";
import { PCForm } from "@/app/components/PCForm";
import { Header } from "@/app/components/Header";
import { Button } from "@/app/components/ui/button";
import { Input } from "@/app/components/ui/input";
import { ScrollArea } from "@/app/components/ui/scroll-area";
import {
  RefreshCw,
  LogIn,
  Search,
  Shield,
  User,
  Plus,
  Pencil,
  Trash2,
} from "lucide-react";
import { useAuth } from "@/contexts/AuthContext";

const relationshipLegend: { type: string; color: string; label: string }[] = [
  { type: 'ally', color: '#10b981', label: 'Ally' },
  { type: 'friend', color: '#34d399', label: 'Friend' },
  { type: 'enemy', color: '#ef4444', label: 'Enemy' },
  { type: 'rival', color: '#f97316', label: 'Rival' },
  { type: 'family', color: '#a855f7', label: 'Family' },
  { type: 'mentor', color: '#3b82f6', label: 'Mentor' },
  { type: 'patron', color: '#0ea5e9', label: 'Patron' },
  { type: 'leader', color: '#4f46e5', label: 'Leader' },
  { type: 'lover', color: '#f43f5e', label: 'Lover' },
  { type: 'neutral', color: '#6b7280', label: 'Other' },
];

export function CampaignPage() {
  const { id } = useParams<{ id: string }>();
  const campaignId = id ? Number(id) : undefined;
  const { isAuthenticated, loginWithCognito, loading: authLoading } = useAuth();

  // Fetch campaign info for breadcrumb
  const { campaigns } = useCampaignData();
  const campaign = campaigns.find(c => c.id === campaignId);

  // Data hooks scoped to this campaign
  const {
    npcs,
    relationships,
    loading: npcLoading,
    error: npcError,
    refreshNpcs,
    saveNPC,
    deleteNPC,
    addRelationship,
    deleteRelationship,
  } = useNPCData(campaignId);

  const {
    pcs,
    loading: pcLoading,
    error: pcError,
    refreshPcs,
    savePc,
    deletePc,
  } = usePCData(campaignId);

  const loading = npcLoading || pcLoading;
  const error = [npcError, pcError].filter(Boolean).join('; ') || null;

  // Entity list combining NPCs and PCs
  const entities = useMemo<EntityItem[]>(() => [
    ...npcs.map((npc): EntityItem => ({
      id: npc.id,
      name: npc.name,
      entityType: 'npc',
      lineage: npc.lineage,
      class: npc.class,
      description: npc.description,
      faction: npc.faction,
      notes: npc.notes,
      campaignId: npc.campaignId,
    })),
    ...pcs.map((pc): EntityItem => ({
      id: pc.id,
      name: pc.name,
      entityType: 'pc',
      description: pc.description,
      campaignId: pc.campaignId,
    })),
  ], [npcs, pcs]);

  const [selectedEntity, setSelectedEntity] = useState<EntityItem | null>(null);
  const [search, setSearch] = useState("");
  const [showNPCs, setShowNPCs] = useState(true);
  const [showPCs, setShowPCs] = useState(true);

  // NPC form state
  const [npcFormOpen, setNpcFormOpen] = useState(false);
  const [editingNPC, setEditingNPC] = useState<NPC | null>(null);

  // PC form state
  const [pcFormOpen, setPcFormOpen] = useState(false);
  const [editingPC, setEditingPC] = useState<PC | null>(null);

  // Canvas sizing
  const canvasContainerRef = useRef<HTMLDivElement>(null);
  const [canvasSize, setCanvasSize] = useState({ width: 800, height: 600 });

  useEffect(() => {
    const container = canvasContainerRef.current;
    if (!container) return;
    const observer = new ResizeObserver(entries => {
      for (const entry of entries) {
        const { width, height } = entry.contentRect;
        setCanvasSize({ width: Math.floor(width), height: Math.floor(height) });
      }
    });
    observer.observe(container);
    return () => observer.disconnect();
  }, []);

  // Filter entities
  const filteredEntities = entities.filter(e => {
    if (e.entityType === 'npc' && !showNPCs) return false;
    if (e.entityType === 'pc' && !showPCs) return false;
    if (search.trim()) {
      return e.name.toLowerCase().includes(search.toLowerCase());
    }
    return true;
  });

  // Filter relationships to visible entities
  const visibleEntityKeys = new Set(filteredEntities.map(e => `${e.entityType}-${e.id}`));
  const filteredRelationships = relationships.filter(rel => {
    const k1 = `${rel.entityType1}-${rel.npcId1}`;
    const k2 = `${rel.entityType2}-${rel.npcId2}`;
    return visibleEntityKeys.has(k1) && visibleEntityKeys.has(k2);
  });

  const handleRefresh = async () => {
    await Promise.all([refreshNpcs(), refreshPcs()]);
  };

  // NPC CRUD handlers
  const handleAddNPC = () => {
    setEditingNPC(null);
    setNpcFormOpen(true);
  };

  const handleEditNPC = (npc: NPC) => {
    setEditingNPC(npc);
    setNpcFormOpen(true);
  };

  const handleDeleteNPC = async (id: number) => {
    if (confirm('Are you sure you want to delete this NPC? All their relationships will also be removed.')) {
      await deleteNPC(id);
      if (selectedEntity?.entityType === 'npc' && selectedEntity?.id === id) {
        setSelectedEntity(null);
      }
    }
  };

  // PC CRUD handlers
  const handleAddPC = () => {
    setEditingPC(null);
    setPcFormOpen(true);
  };

  const handleEditPC = (pc: PC) => {
    setEditingPC(pc);
    setPcFormOpen(true);
  };

  const handleDeletePC = async (id: number) => {
    if (confirm('Are you sure you want to delete this character?')) {
      await deletePc(id);
      if (selectedEntity?.entityType === 'pc' && selectedEntity?.id === id) {
        setSelectedEntity(null);
      }
    }
  };

  // Handle edit/delete from the detail panel
  const handleEditEntity = () => {
    if (!selectedEntity) return;
    if (selectedEntity.entityType === 'npc') {
      const npc = npcs.find(n => n.id === selectedEntity.id);
      if (npc) handleEditNPC(npc);
    } else {
      const pc = pcs.find(p => p.id === selectedEntity.id);
      if (pc) handleEditPC(pc);
    }
  };

  const handleDeleteEntity = () => {
    if (!selectedEntity) return;
    if (selectedEntity.entityType === 'npc') {
      handleDeleteNPC(selectedEntity.id);
    } else {
      handleDeletePC(selectedEntity.id);
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

  if (!campaignId) {
    return <Navigate to="/relationship-manager" replace />;
  }

  return (
    <div className="h-screen overflow-hidden bg-gradient-to-br from-background via-background to-secondary/20 flex flex-col">
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

      <div className="container mx-auto py-8 px-4 relative flex flex-col flex-1 min-h-0">
        <Header
          breadcrumbs={[
            { label: "Relationship Manager", to: "/relationship-manager" },
            { label: campaign?.name || "Campaign" },
          ]}
        />

        {/* Top controls bar */}
        <div className="flex items-center justify-between mb-4 gap-4 flex-wrap">
          <h2 className="text-2xl font-bold bg-gradient-to-r from-primary to-accent bg-clip-text text-transparent">
            {campaign?.name || "Campaign"}
          </h2>

          <div className="flex items-center gap-3 flex-wrap">
            {error && (
              <span className="text-sm text-destructive">{error}</span>
            )}
            {loading && (
              <RefreshCw className="size-4 animate-spin text-muted-foreground" />
            )}
            <Button
              variant="ghost"
              size="icon"
              onClick={handleRefresh}
              disabled={loading}
              title="Refresh"
              aria-label="Refresh"
            >
              <RefreshCw className={`size-4 ${loading ? 'animate-spin' : ''}`} />
            </Button>

            {/* Entity type toggles */}
            <div className="flex gap-2">
              <Button
                size="sm"
                variant={showNPCs ? "default" : "outline"}
                onClick={() => setShowNPCs(v => !v)}
                className={showNPCs
                  ? "bg-primary/80 hover:bg-primary/70 text-primary-foreground"
                  : "border-primary/30 text-primary hover:bg-primary/10"
                }
              >
                <Shield className="size-3 mr-1" />
                NPCs
              </Button>
              <Button
                size="sm"
                variant={showPCs ? "default" : "outline"}
                onClick={() => setShowPCs(v => !v)}
                className={showPCs
                  ? "bg-green-600/80 hover:bg-green-600/70 text-white"
                  : "border-green-500/30 text-green-400 hover:bg-green-500/10"
                }
              >
                <User className="size-3 mr-1" />
                PCs
              </Button>
            </div>
          </div>
        </div>

        {/* Main 3-column layout */}
        {!isAuthenticated ? (
          <div className="text-center py-20">
            <div className="bg-gradient-to-br from-card to-secondary/30 border border-primary/30 rounded-2xl p-12 max-w-md mx-auto shadow-xl">
              <div className="bg-gradient-to-br from-primary/20 to-accent/20 w-24 h-24 rounded-full flex items-center justify-center mx-auto mb-6">
                <LogIn className="size-12 text-primary" />
              </div>
              <h3 className="text-2xl font-bold mb-3 text-primary">Sign In Required</h3>
              <p className="text-muted-foreground mb-6 leading-relaxed">
                Sign in to view and manage this campaign
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
          <div className="flex gap-4 flex-1 min-h-0">
            {/* LEFT SIDEBAR — entity list */}
            <div className="w-56 shrink-0 flex flex-col bg-card/50 border border-primary/20 rounded-2xl overflow-hidden">
              <div className="p-3 border-b border-primary/20">
                <div className="relative">
                  <Search className="absolute left-2.5 top-1/2 -translate-y-1/2 size-3.5 text-muted-foreground" />
                  <Input
                    value={search}
                    onChange={e => setSearch(e.target.value)}
                    placeholder="Search..."
                    className="pl-8 h-8 text-sm bg-background/50 border-primary/20"
                  />
                </div>
              </div>
              <ScrollArea className="flex-1">
                <div className="p-2 space-y-0.5">
                  {loading ? (
                    <div className="flex items-center justify-center py-8">
                      <RefreshCw className="size-5 animate-spin text-primary" />
                    </div>
                  ) : filteredEntities.length === 0 ? (
                    <p className="text-xs text-muted-foreground text-center py-6 px-2">
                      {search ? 'No entities match your search.' : 'No entities yet. Add NPCs or PCs to get started.'}
                    </p>
                  ) : (
                    filteredEntities.map(entity => {
                      const isSelected =
                        selectedEntity?.id === entity.id &&
                        selectedEntity?.entityType === entity.entityType;
                      const isNpc = entity.entityType === 'npc';
                      return (
                        <button
                          key={`${entity.entityType}-${entity.id}`}
                          onClick={() => setSelectedEntity(isSelected ? null : entity)}
                          className={`w-full text-left flex items-center gap-2 px-2 py-1.5 rounded-lg text-sm transition-colors ${
                            isSelected
                              ? 'bg-primary/20 text-primary'
                              : 'hover:bg-primary/10 text-foreground'
                          }`}
                        >
                          {isNpc
                            ? <Shield className={`size-3.5 shrink-0 ${isSelected ? 'text-primary' : 'text-primary/60'}`} />
                            : <User className={`size-3.5 shrink-0 ${isSelected ? 'text-green-400' : 'text-green-400/60'}`} />
                          }
                          <span className="truncate">{entity.name}</span>
                        </button>
                      );
                    })
                  )}
                </div>
              </ScrollArea>

              {/* Add buttons + counts */}
              <div className="p-2 border-t border-primary/20 space-y-2">
                <div className="flex gap-1">
                  <Button
                    size="sm"
                    variant="outline"
                    onClick={handleAddNPC}
                    className="flex-1 h-7 text-xs border-primary/30 hover:bg-primary/10"
                  >
                    <Plus className="size-3 mr-1" />
                    NPC
                  </Button>
                  <Button
                    size="sm"
                    variant="outline"
                    onClick={handleAddPC}
                    className="flex-1 h-7 text-xs border-green-500/30 hover:bg-green-500/10 text-green-400"
                  >
                    <Plus className="size-3 mr-1" />
                    PC
                  </Button>
                </div>
                <div className="flex gap-2 text-xs text-muted-foreground px-1">
                  <span>{npcs.length} NPCs</span>
                  <span>·</span>
                  <span>{pcs.length} PCs</span>
                </div>
              </div>
            </div>

            {/* CENTER — graph canvas + legend */}
            <div className="flex-1 flex flex-col min-w-0 gap-2">
              <div ref={canvasContainerRef} className="flex-1 min-h-0 overflow-hidden">
                <EntityGraph
                  entities={filteredEntities}
                  relationships={filteredRelationships}
                  selectedEntityId={selectedEntity?.id}
                  selectedEntityType={selectedEntity?.entityType}
                  onNodeClick={entity => setSelectedEntity(prev =>
                    prev?.id === entity.id && prev?.entityType === entity.entityType ? null : entity
                  )}
                  width={canvasSize.width}
                  height={canvasSize.height}
                />
              </div>

              {/* Legend */}
              <div className="flex items-center gap-3 flex-wrap px-2 py-1.5 bg-card/40 border border-primary/20 rounded-xl">
                <span className="text-xs font-semibold text-muted-foreground uppercase tracking-wider">
                  Relationships:
                </span>
                {relationshipLegend.map(({ type, color, label }) => (
                  <div key={type} className="flex items-center gap-1.5">
                    <span
                      className="inline-block w-2.5 h-2.5 rounded-full shrink-0"
                      style={{ backgroundColor: color }}
                    />
                    <span className="text-xs text-muted-foreground">{label}</span>
                  </div>
                ))}
                <div className="flex items-center gap-1.5 ml-2 border-l border-primary/20 pl-2">
                  <span className="inline-block w-2.5 h-2.5 rounded-full bg-primary/70 shrink-0" />
                  <span className="text-xs text-muted-foreground">NPC</span>
                </div>
                <div className="flex items-center gap-1.5">
                  <span className="inline-block w-2.5 h-2.5 rounded-full bg-green-500/70 shrink-0" />
                  <span className="text-xs text-muted-foreground">PC</span>
                </div>
              </div>
            </div>

            {/* RIGHT SIDEBAR — entity detail + actions */}
            <div className="w-72 shrink-0 bg-card/50 border border-primary/20 rounded-2xl overflow-hidden flex flex-col min-h-0">
              <div className="flex-1 min-h-0">
                <EntityDetailPanel
                  entity={selectedEntity}
                  relationships={relationships}
                  allEntities={entities}
                  onAddRelationship={addRelationship}
                  onDeleteRelationship={deleteRelationship}
                />
              </div>
              {/* Edit/Delete actions for selected entity */}
              {selectedEntity && (
                <div className="p-3 border-t border-primary/20 flex gap-2">
                  <Button
                    size="sm"
                    variant="outline"
                    onClick={handleEditEntity}
                    className="flex-1 hover:bg-primary/10 hover:border-primary/50"
                  >
                    <Pencil className="size-3 mr-1" />
                    Edit
                  </Button>
                  <Button
                    size="sm"
                    variant="outline"
                    onClick={handleDeleteEntity}
                    className="hover:bg-destructive/10 hover:border-destructive/50 hover:text-destructive"
                    aria-label="Delete entity"
                  >
                    <Trash2 className="size-3" />
                  </Button>
                </div>
              )}
            </div>
          </div>
        )}
      </div>

      {/* Form Modals */}
      <NPCForm
        open={npcFormOpen}
        onOpenChange={setNpcFormOpen}
        onSave={saveNPC}
        editingNPC={editingNPC}
      />
      <PCForm
        open={pcFormOpen}
        onOpenChange={setPcFormOpen}
        onSave={savePc}
        editingPC={editingPC}
        campaignId={campaignId}
      />
    </div>
  );
}
