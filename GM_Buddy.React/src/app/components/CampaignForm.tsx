import { useState, useEffect } from "react";
import { Campaign } from "@/types/campaign";
import { gameSystemApi, ApiGameSystem } from "@/services/api";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/app/components/ui/dialog";
import { Button } from "@/app/components/ui/button";
import { Input } from "@/app/components/ui/input";
import { Label } from "@/app/components/ui/label";
import { Textarea } from "@/app/components/ui/textarea";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/app/components/ui/select";

interface CampaignFormProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSave: (campaign: Omit<Campaign, 'id'> | Campaign) => void;
  editingCampaign?: Campaign | null;
}

export function CampaignForm({ open, onOpenChange, onSave, editingCampaign }: CampaignFormProps) {
  const [formData, setFormData] = useState({
    name: "",
    description: "",
    gameSystemId: 0,
  });

  const [gameSystems, setGameSystems] = useState<ApiGameSystem[]>([]);
  const [loadingGameSystems, setLoadingGameSystems] = useState(false);

  useEffect(() => {
    const loadGameSystems = async () => {
      try {
        setLoadingGameSystems(true);
        const systems = await gameSystemApi.getGameSystems();
        setGameSystems(systems);
        
        // Set default game system if none selected and systems loaded
        if (formData.gameSystemId === 0 && systems.length > 0) {
          setFormData(prev => ({
            ...prev,
            gameSystemId: systems[0].game_system_id
          }));
        }
      } catch (error) {
        console.error('Failed to load game systems:', error);
      } finally {
        setLoadingGameSystems(false);
      }
    };

    if (open) {
      loadGameSystems();
    }
  }, [open]);

  useEffect(() => {
    if (editingCampaign) {
      setFormData({
        name: editingCampaign.name,
        description: editingCampaign.description || "",
        gameSystemId: editingCampaign.gameSystemId,
      });
    } else {
      setFormData({
        name: "",
        description: "",
        gameSystemId: gameSystems.length > 0 ? gameSystems[0].game_system_id : 0,
      });
    }
  }, [editingCampaign, open, gameSystems]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    const selectedGameSystem = gameSystems.find(gs => gs.game_system_id === formData.gameSystemId);
    
    try {
      if (editingCampaign) {
        await onSave({ 
          ...editingCampaign, 
          ...formData,
          gameSystemName: selectedGameSystem?.game_system_name
        });
      } else {
        await onSave({
          ...formData,
          gameSystemName: selectedGameSystem?.game_system_name
        });
      }
      
      onOpenChange(false);
    } catch (error) {
      // Let the error propagate to be handled by parent component
      console.error('Failed to save campaign:', error);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl">
        <DialogHeader>
          <DialogTitle>{editingCampaign ? "Edit Campaign" : "Create New Campaign"}</DialogTitle>
          <DialogDescription>
            {editingCampaign ? "Update the details of your campaign." : "Add a new campaign to manage."}
          </DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit}>
          <div className="grid gap-4 py-4">
            <div className="grid gap-2">
              <Label htmlFor="name">Name *</Label>
              <Input
                id="name"
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                placeholder="Enter campaign name"
                required
              />
            </div>

            <div className="grid gap-2">
              <Label htmlFor="description">Description</Label>
              <Textarea
                id="description"
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                placeholder="Describe your campaign..."
                rows={4}
              />
            </div>

            <div className="grid gap-2">
              <Label htmlFor="gameSystem">Game System *</Label>
              <Select
                value={formData.gameSystemId.toString()}
                onValueChange={(value) => setFormData({ ...formData, gameSystemId: parseInt(value) })}
                disabled={loadingGameSystems}
              >
                <SelectTrigger id="gameSystem">
                  <SelectValue placeholder={loadingGameSystems ? "Loading game systems..." : "Select a game system"} />
                </SelectTrigger>
                <SelectContent>
                  {gameSystems.map((system) => (
                    <SelectItem 
                      key={system.game_system_id} 
                      value={system.game_system_id.toString()}
                    >
                      {system.game_system_name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>

          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
              Cancel
            </Button>
            <Button 
              type="submit"
              className="bg-gradient-to-r from-primary to-accent hover:from-primary/90 hover:to-accent/90"
              disabled={!formData.name || formData.gameSystemId === 0}
            >
              {editingCampaign ? "Update Campaign" : "Create Campaign"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
