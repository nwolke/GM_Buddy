import { useState, useEffect } from "react";
import { Campaign } from "@/types/campaign";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/app/components/ui/dialog";
import { Button } from "@/app/components/ui/button";
import { Input } from "@/app/components/ui/input";
import { Label } from "@/app/components/ui/label";
import { Textarea } from "@/app/components/ui/textarea";

interface CampaignFormProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSave: (campaign: Omit<Campaign, 'id'> | Campaign) => Promise<void>;
  editingCampaign?: Campaign | null;
}

export function CampaignForm({ open, onOpenChange, onSave, editingCampaign }: CampaignFormProps) {
  const [formData, setFormData] = useState({
    name: "",
    description: "",
  });

  useEffect(() => {
    if (editingCampaign) {
      setFormData({
        name: editingCampaign.name,
        description: editingCampaign.description || "",
      });
    } else {
      setFormData({
        name: "",
        description: "",
      });
    }
  }, [editingCampaign, open]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    try {
      if (editingCampaign) {
        await onSave({ 
          ...editingCampaign, 
          ...formData
        });
      } else {
        await onSave(formData);
      }
      
      onOpenChange(false);
    } catch (error) {
      // Error is logged but dialog remains open so user can retry
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
          </div>

          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
              Cancel
            </Button>
            <Button 
              type="submit"
              className="bg-gradient-to-r from-primary to-accent hover:from-primary/90 hover:to-accent/90"
              disabled={!formData.name}
            >
              {editingCampaign ? "Update Campaign" : "Create Campaign"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
