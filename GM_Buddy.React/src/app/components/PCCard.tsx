import { PC } from "@/types/pc";
import { Card, CardContent, CardHeader, CardTitle } from "@/app/components/ui/card";
import { Button } from "@/app/components/ui/button";
import { Pencil, Trash2, User } from "lucide-react";

interface PCCardProps {
  pc: PC;
  onEdit: (pc: PC) => void;
  onDelete: (id: number) => void;
}

export function PCCard({ pc, onEdit, onDelete }: PCCardProps) {
  return (
    <Card className="hover:shadow-2xl hover:shadow-primary/20 transition-all hover:scale-[1.02] bg-gradient-to-br from-card to-card/80 border-primary/30 relative overflow-hidden group">
      {/* Decorative corner accent */}
      <div className="absolute top-0 right-0 w-20 h-20 bg-gradient-to-br from-primary/20 to-transparent rounded-bl-full" />
      <div className="absolute bottom-0 left-0 w-16 h-16 bg-gradient-to-tr from-accent/10 to-transparent rounded-tr-full" />

      <CardHeader className="relative">
        <div className="flex items-start justify-between">
          <div className="flex-1">
            <CardTitle className="text-xl flex items-center gap-2">
              <User className="size-5 text-primary" />
              {pc.name}
            </CardTitle>
          </div>
          <div className="flex gap-1">
            <Button
              variant="ghost"
              size="icon"
              onClick={() => onEdit(pc)}
              aria-label="Edit PC"
              className="hover:bg-accent/20 hover:text-accent"
            >
              <Pencil className="size-4" />
            </Button>
            <Button
              variant="ghost"
              size="icon"
              onClick={() => onDelete(pc.id)}
              aria-label="Delete PC"
              className="hover:bg-destructive/20 hover:text-destructive"
            >
              <Trash2 className="size-4" />
            </Button>
          </div>
        </div>
      </CardHeader>

      <CardContent className="space-y-3 relative">
        {pc.description && (
          <div>
            <span className="text-xs font-semibold text-muted-foreground uppercase tracking-wider">Description</span>
            <p className="text-sm text-foreground/90 mt-1 leading-relaxed">{pc.description}</p>
          </div>
        )}
        {pc.createdAt && (
          <div className="border-t border-border/50 pt-3">
            <span className="text-xs text-muted-foreground">
              Created {new Date(pc.createdAt).toLocaleDateString()}
            </span>
          </div>
        )}
      </CardContent>
    </Card>
  );
}
