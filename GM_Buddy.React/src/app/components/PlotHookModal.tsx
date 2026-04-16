import { useState } from "react";
import { EntityItem } from "@/types/entity";
import { plotHookApi, PlotHook } from "@/services/api";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/app/components/ui/dialog";
import { Button } from "@/app/components/ui/button";
import { Label } from "@/app/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/app/components/ui/select";
import { Badge } from "@/app/components/ui/badge";
import { ScrollArea } from "@/app/components/ui/scroll-area";
import { Separator } from "@/app/components/ui/separator";
import { RefreshCw, Sparkles, Shield, User, ChevronRight } from "lucide-react";

const TONES = [
  { value: "adventure", label: "Adventure" },
  { value: "mystery", label: "Mystery" },
  { value: "political", label: "Political" },
  { value: "horror", label: "Horror" },
  { value: "intrigue", label: "Intrigue" },
  { value: "romance", label: "Romance" },
  { value: "comedy", label: "Comedy" },
  { value: "tragedy", label: "Tragedy" },
] as const;

const HOOK_COUNTS = [3, 5, 10] as const;
type HookCount = (typeof HOOK_COUNTS)[number];

interface PlotHookModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  campaignId: number;
  entities: EntityItem[];
}

type ModalView = "form" | "loading" | "results";

export function PlotHookModal({
  open,
  onOpenChange,
  campaignId,
  entities,
}: PlotHookModalProps) {
  const [view, setView] = useState<ModalView>("form");
  const [selectedTones, setSelectedTones] = useState<string[]>([]);
  const [hookCount, setHookCount] = useState<HookCount>(3);
  const [focusEntityKey, setFocusEntityKey] = useState<string>("");
  const [hooks, setHooks] = useState<PlotHook[]>([]);
  const [error, setError] = useState<string | null>(null);

  const npcs = entities.filter((e) => e.entityType === "npc");
  const pcs = entities.filter((e) => e.entityType === "pc");

  const toggleTone = (tone: string) => {
    setSelectedTones((prev) =>
      prev.includes(tone) ? prev.filter((t) => t !== tone) : [...prev, tone]
    );
  };

  const parseFocusEntityKey = (
    key: string
  ): { id: number; type: "npc" | "pc" } | null => {
    if (!key) return null;
    const parts = key.split("-");
    if (parts.length !== 2) return null;
    const [entityType, idStr] = parts;
    if (entityType !== "npc" && entityType !== "pc") return null;
    const id = Number(idStr);
    if (Number.isNaN(id)) return null;
    return { id, type: entityType };
  };

  const handleGenerate = async () => {
    setError(null);
    setView("loading");

    const focus = parseFocusEntityKey(focusEntityKey);

    try {
      const response = await plotHookApi.generatePlotHooks(campaignId, {
        tones: selectedTones,
        hookCount,
        focusEntityId: focus?.id,
        focusEntityType: focus?.type,
      });
      setHooks(response.hooks);
      setView("results");
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "Failed to generate plot hooks. Please try again."
      );
      setView("form");
    }
  };

  const handleRegenerate = () => {
    setView("form");
    setHooks([]);
  };

  const handleClose = () => {
    setView("form");
    setSelectedTones([]);
    setHookCount(3);
    setFocusEntityKey("");
    setHooks([]);
    setError(null);
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent className="max-w-2xl max-h-[90vh] flex flex-col overflow-hidden">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <Sparkles className="size-5 text-primary" />
            Generate Plot Hooks
          </DialogTitle>
          <DialogDescription>
            {view === "results"
              ? "AI-generated plot hooks based on your campaign's characters and relationships."
              : "Configure options to generate context-aware plot hooks for your campaign."}
          </DialogDescription>
        </DialogHeader>

        {/* FORM VIEW */}
        {view === "form" && (
          <div className="flex flex-col gap-5 py-2 flex-1 overflow-y-auto">
            {/* Tone / Theme */}
            <div className="grid gap-2">
              <Label>
                Tone / Theme
                <span className="text-muted-foreground font-normal ml-1 text-xs">
                  (select any that apply)
                </span>
              </Label>
              <div className="flex flex-wrap gap-2">
                {TONES.map(({ value, label }) => {
                  const selected = selectedTones.includes(value);
                  return (
                    <button
                      key={value}
                      type="button"
                      onClick={() => toggleTone(value)}
                      className={`px-3 py-1.5 rounded-full text-sm border transition-colors ${
                        selected
                          ? "bg-primary/20 border-primary/60 text-primary"
                          : "border-border text-muted-foreground hover:border-primary/40 hover:text-foreground"
                      }`}
                    >
                      {label}
                    </button>
                  );
                })}
              </div>
            </div>

            {/* Hook count */}
            <div className="grid gap-2">
              <Label htmlFor="hook-count">Number of Hooks</Label>
              <Select
                value={hookCount.toString()}
                onValueChange={(v) => setHookCount(Number(v) as HookCount)}
              >
                <SelectTrigger id="hook-count" className="w-32">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {HOOK_COUNTS.map((n) => (
                    <SelectItem key={n} value={n.toString()}>
                      {n}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            {/* Focus entity */}
            <div className="grid gap-2">
              <Label htmlFor="focus-entity">
                Focus Entity
                <span className="text-muted-foreground font-normal ml-1 text-xs">
                  (optional — center hooks on a specific character)
                </span>
              </Label>
              <Select value={focusEntityKey} onValueChange={setFocusEntityKey}>
                <SelectTrigger id="focus-entity">
                  <SelectValue placeholder="No focus — use all characters" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="">No focus</SelectItem>
                  {npcs.length > 0 && (
                    <>
                      <div className="px-2 py-1 text-xs font-semibold text-muted-foreground uppercase tracking-wider flex items-center gap-1">
                        <Shield className="size-3" />
                        NPCs
                      </div>
                      {npcs.map((e) => (
                        <SelectItem key={`npc-${e.id}`} value={`npc-${e.id}`}>
                          {e.name}
                          {e.lineage && e.class && (
                            <span className="text-muted-foreground">
                              {" "}
                              ({e.lineage} {e.class})
                            </span>
                          )}
                        </SelectItem>
                      ))}
                    </>
                  )}
                  {pcs.length > 0 && (
                    <>
                      <div className="px-2 py-1 text-xs font-semibold text-muted-foreground uppercase tracking-wider flex items-center gap-1">
                        <User className="size-3" />
                        Player Characters
                      </div>
                      {pcs.map((e) => (
                        <SelectItem key={`pc-${e.id}`} value={`pc-${e.id}`}>
                          {e.name}
                        </SelectItem>
                      ))}
                    </>
                  )}
                </SelectContent>
              </Select>
            </div>

            {error && (
              <p className="text-sm text-destructive bg-destructive/10 border border-destructive/30 rounded-lg px-3 py-2">
                {error}
              </p>
            )}
          </div>
        )}

        {/* LOADING VIEW */}
        {view === "loading" && (
          <div className="flex flex-col items-center justify-center py-16 gap-4 flex-1">
            <div className="bg-gradient-to-br from-primary/20 to-accent/20 w-20 h-20 rounded-full flex items-center justify-center">
              <RefreshCw className="size-8 text-primary animate-spin" />
            </div>
            <p className="text-muted-foreground text-center">
              Generating plot hooks…
              <br />
              <span className="text-xs">
                This may take a moment while the AI analyzes your campaign.
              </span>
            </p>
          </div>
        )}

        {/* RESULTS VIEW */}
        {view === "results" && hooks.length > 0 && (
          <ScrollArea className="flex-1 pr-1">
            <div className="space-y-4 py-2">
              {hooks.map((hook, i) => (
                <div
                  key={i}
                  className="rounded-xl border border-primary/20 bg-card/60 p-4 space-y-3"
                >
                  <div className="flex items-start gap-2">
                    <span className="shrink-0 w-6 h-6 rounded-full bg-primary/20 text-primary text-xs font-bold flex items-center justify-center mt-0.5">
                      {i + 1}
                    </span>
                    <h3 className="font-semibold text-base text-primary leading-tight">
                      {hook.title}
                    </h3>
                  </div>

                  <p className="text-sm text-foreground/90 leading-relaxed pl-8">
                    {hook.setup}
                  </p>

                  {hook.involvedEntities.length > 0 && (
                    <div className="pl-8 flex flex-wrap gap-1.5">
                      {hook.involvedEntities.map((name) => (
                        <Badge
                          key={name}
                          variant="outline"
                          className="text-xs border-accent/40 text-accent"
                        >
                          {name}
                        </Badge>
                      ))}
                    </div>
                  )}

                  <Separator className="bg-primary/10" />

                  <div className="pl-8 space-y-2">
                    <div>
                      <p className="text-xs font-semibold text-muted-foreground uppercase tracking-wider mb-0.5">
                        Conflict
                      </p>
                      <p className="text-sm text-foreground/80">{hook.conflict}</p>
                    </div>
                    <div>
                      <p className="text-xs font-semibold text-muted-foreground uppercase tracking-wider mb-0.5 flex items-center gap-1">
                        <ChevronRight className="size-3" />
                        Next Scene
                      </p>
                      <p className="text-sm text-foreground/80 italic">
                        {hook.nextScene}
                      </p>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </ScrollArea>
        )}

        <DialogFooter className="mt-4 gap-2 flex-row flex-wrap">
          {view === "results" && (
            <Button
              variant="outline"
              onClick={handleRegenerate}
              className="gap-2"
            >
              <RefreshCw className="size-4" />
              Regenerate
            </Button>
          )}
          <div className="flex-1" />
          <Button variant="outline" onClick={handleClose}>
            {view === "results" ? "Close" : "Cancel"}
          </Button>
          {view === "form" && (
            <Button
              onClick={handleGenerate}
              className="bg-gradient-to-r from-primary to-accent hover:from-primary/90 hover:to-accent/90 shadow-lg shadow-primary/20 gap-2"
            >
              <Sparkles className="size-4" />
              Generate
            </Button>
          )}
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
