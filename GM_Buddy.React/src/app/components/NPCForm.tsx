import { useState, useEffect } from "react";
import { NPC } from "@/types/npc";
import { campaignApi, Campaign } from "@/services/api";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/app/components/ui/dialog";
import { Button } from "@/app/components/ui/button";
import { Input } from "@/app/components/ui/input";
import { Label } from "@/app/components/ui/label";
import { Textarea } from "@/app/components/ui/textarea";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/app/components/ui/select";
import { AlertTriangle } from "lucide-react";

interface NPCFormProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSave: (npc: Omit<NPC, 'id'> | NPC) => void;
  editingNPC?: NPC | null;
}

export function NPCForm({ open, onOpenChange, onSave, editingNPC }: NPCFormProps) {
const [formData, setFormData] = useState({
  name: "",
  race: "",
  class: "",
  description: "",
  campaignId: undefined as number | undefined,
  faction: "",
  notes: ""
});

const [campaigns, setCampaigns] = useState<Campaign[]>([]);
const [loadingCampaigns, setLoadingCampaigns] = useState(false);
const [originalCampaignId, setOriginalCampaignId] = useState<number | undefined>(undefined);
const [showCampaignChangeWarning, setShowCampaignChangeWarning] = useState(false);

  // Load campaigns when the dialog is opened
  useEffect(() => {
    const loadCampaigns = async () => {
      try {
        setLoadingCampaigns(true);
        const userCampaigns = await campaignApi.getCampaignsByAccount();
        setCampaigns(userCampaigns);
      } catch (error) {
        console.error('Failed to load campaigns:', error);
      } finally {
        setLoadingCampaigns(false);
      }
    };

    if (open) {
      loadCampaigns();
    }
  }, [open]);

  useEffect(() => {
    if (editingNPC) {
      setFormData({
        name: editingNPC.name,
        race: editingNPC.race,
        class: editingNPC.class,
        description: editingNPC.description,
        campaignId: editingNPC.campaignId,
        faction: editingNPC.faction || "",
        notes: editingNPC.notes || ""
      });
      setOriginalCampaignId(editingNPC.campaignId);
    } else {
      setFormData({
        name: "",
        race: "",
        class: "",
        description: "",
        campaignId: undefined,
        faction: "",
        notes: ""
      });
      setOriginalCampaignId(undefined);
    }
  }, [editingNPC, open, campaigns]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!formData.campaignId) {
      alert('Please select a campaign');
      return;
    }
    
    // Check if campaign changed when editing
    if (editingNPC && originalCampaignId !== formData.campaignId) {
      setShowCampaignChangeWarning(true);
      return;
    }
    
    submitForm();
  };

  const submitForm = () => {
    if (editingNPC) {
      onSave({ ...editingNPC, ...formData });
    } else {
      onSave(formData as Omit<NPC, 'id'>);
    }
    
    setShowCampaignChangeWarning(false);
    onOpenChange(false);
  };

  const handleCampaignChange = (campaignId: string) => {
    const numericId = parseInt(campaignId);
    setFormData({ ...formData, campaignId: numericId });
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>{editingNPC ? "Edit NPC" : "Create New NPC"}</DialogTitle>
          <DialogDescription>
            {editingNPC ? "Update the details of your NPC." : "Add a new NPC to your campaign."}
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
                placeholder="Enter NPC name"
                required
              />
            </div>
            <div className="grid gap-2">
              <Label htmlFor="campaign">Campaign *</Label>
              <Select
                value={formData.campaignId?.toString() || ""}
                onValueChange={handleCampaignChange}
                disabled={loadingCampaigns}
              >
                <SelectTrigger id="campaign">
                  <SelectValue placeholder="Select a campaign" />
                </SelectTrigger>
                <SelectContent>
                  {campaigns.map((campaign) => (
                    <SelectItem key={campaign.id} value={campaign.id.toString()}>
                      {campaign.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              {editingNPC && originalCampaignId !== formData.campaignId && formData.campaignId && (
                <div className="flex items-start gap-2 p-3 bg-yellow-500/10 border border-yellow-500/50 rounded-md">
                  <AlertTriangle className="size-5 text-yellow-600 mt-0.5 flex-shrink-0" />
                  <p className="text-sm text-yellow-700 dark:text-yellow-500">
                    <strong>Warning:</strong> Moving this NPC to a different campaign will remove all their relationships.
                  </p>
                </div>
              )}
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="grid gap-2">
                <Label htmlFor="race">Race *</Label>
                <Input
                  id="race"
                  value={formData.race}
                  onChange={(e) => setFormData({ ...formData, race: e.target.value })}
                  placeholder="e.g., Human, Elf, Dwarf"
                  required
                />
              </div>
              <div className="grid gap-2">
                <Label htmlFor="class">Class *</Label>
                <Input
                  id="class"
                  value={formData.class}
                  onChange={(e) => setFormData({ ...formData, class: e.target.value })}
                  placeholder="e.g., Warrior, Mage, Rogue"
                  required
                />
              </div>
            </div>
            <div className="grid gap-2">
              <Label htmlFor="faction">Faction</Label>
              <Input
                id="faction"
                value={formData.faction}
                onChange={(e) => setFormData({ ...formData, faction: e.target.value })}
                placeholder="e.g., The Iron Guard, Merchants Guild"
              />
            </div>
            <div className="grid gap-2">
              <Label htmlFor="description">Description *</Label>
              <Textarea
                id="description"
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                placeholder="Describe this NPC's appearance, personality, or role"
                rows={3}
                required
              />
            </div>
            <div className="grid gap-2">
              <Label htmlFor="notes">Notes</Label>
              <Textarea
                id="notes"
                value={formData.notes}
                onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
                placeholder="Additional notes, plot hooks, or important information"
                rows={3}
              />
            </div>
          </div>
          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
              Cancel
            </Button>
            <Button type="submit">
              {editingNPC ? "Update" : "Create"} NPC
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>

      {/* Campaign Change Warning Dialog */}
      <Dialog open={showCampaignChangeWarning} onOpenChange={setShowCampaignChangeWarning}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle className="flex items-center gap-2">
              <AlertTriangle className="size-6 text-yellow-600" />
              Confirm Campaign Change
            </DialogTitle>
            <DialogDescription>
              Moving this NPC to a different campaign will <strong>permanently delete all their relationships</strong>. This action cannot be undone.
            </DialogDescription>
          </DialogHeader>
          <div className="py-4">
            <p className="text-sm text-muted-foreground">
              From: <strong>{campaigns.find(c => c.id === originalCampaignId)?.name}</strong>
            </p>
            <p className="text-sm text-muted-foreground">
              To: <strong>{campaigns.find(c => c.id === formData.campaignId)?.name}</strong>
            </p>
          </div>
          <DialogFooter>
            <Button 
              type="button" 
              variant="outline" 
              onClick={() => setShowCampaignChangeWarning(false)}
            >
              Cancel
            </Button>
            <Button 
              type="button" 
              variant="destructive"
              onClick={submitForm}
            >
              Confirm & Move NPC
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </Dialog>
  );
}
