import { NPC } from "@/types/npc";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/app/components/ui/card";
import { Button } from "@/app/components/ui/button";
import { Pencil, Trash2, Users, Shield, Sparkles } from "lucide-react";

interface NPCCardProps {
  npc: NPC;
  onEdit: (npc: NPC) => void;
  onDelete: (id: number) => void;
  onManageRelationships: (npc: NPC) => void;
  relationshipCount: number;
}

export function NPCCard({ npc, onEdit, onDelete, onManageRelationships, relationshipCount }: NPCCardProps) {
  return (
    <Card className="hover:shadow-2xl hover:shadow-primary/20 transition-all hover:scale-[1.02] bg-gradient-to-br from-card to-card/80 border-primary/30 relative overflow-hidden group">
      {/* Decorative corner accent */}
      <div className="absolute top-0 right-0 w-20 h-20 bg-gradient-to-br from-primary/20 to-transparent rounded-bl-full" />
      <div className="absolute bottom-0 left-0 w-16 h-16 bg-gradient-to-tr from-accent/10 to-transparent rounded-tr-full" />
      
      <CardHeader className="relative">
        <div className="flex items-start justify-between">
          <div className="flex-1">
            <CardTitle className="text-xl flex items-center gap-2">
              <Shield className="size-5 text-primary" />
              {npc.name}
            </CardTitle>
            <CardDescription className="flex items-center gap-2 mt-1">
              <Sparkles className="size-3 text-accent" />
              <span className="text-primary font-semibold">{npc.race}</span>
              <span className="text-muted-foreground">•</span>
              <span className="text-accent font-semibold">{npc.class}</span>
            </CardDescription>
          </div>
          <div className="flex gap-1">
            <Button
              variant="ghost"
              size="icon"
              onClick={() => onManageRelationships(npc)}
              title="Manage relationships"
              className="hover:bg-primary/20 hover:text-primary"
            >
              <Users className="size-4" />
            </Button>
            <Button
              variant="ghost"
              size="icon"
              onClick={() => onEdit(npc)}
              className="hover:bg-accent/20 hover:text-accent"
            >
              <Pencil className="size-4" />
            </Button>
            <Button
              variant="ghost"
              size="icon"
              onClick={() => onDelete(npc.id)}
              className="hover:bg-destructive/20 hover:text-destructive"
            >
              <Trash2 className="size-4" />
            </Button>
          </div>
        </div>
      </CardHeader>
      <CardContent className="space-y-3 relative">
        {npc.faction && (
          <div className="bg-secondary/50 rounded-lg p-2 border border-primary/20">
            <span className="text-xs font-semibold text-primary uppercase tracking-wider">Faction</span>
            <p className="text-sm text-foreground mt-0.5">{npc.faction}</p>
          </div>
        )}
        {npc.description && (
          <div>
            <span className="text-xs font-semibold text-muted-foreground uppercase tracking-wider">Description</span>
            <p className="text-sm text-foreground/90 mt-1 leading-relaxed">{npc.description}</p>
          </div>
        )}
        {npc.notes && (
          <div className="border-t border-border/50 pt-3">
            <span className="text-xs font-semibold text-muted-foreground uppercase tracking-wider">Notes</span>
            <p className="text-sm text-foreground/80 mt-1 italic leading-relaxed">{npc.notes}</p>
          </div>
        )}
        {relationshipCount > 0 && (
          <div className="pt-2 border-t border-primary/20">
            <div className="flex items-center gap-2 text-sm">
              <Users className="size-4 text-primary" />
              <span className="text-primary font-semibold">
                {relationshipCount} connection{relationshipCount !== 1 ? 's' : ''}
              </span>
            </div>
          </div>
        )}
      </CardContent>
    </Card>
  );
}