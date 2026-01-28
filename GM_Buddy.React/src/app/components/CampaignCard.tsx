import { Campaign } from "@/types/campaign";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/app/components/ui/card";
import { Button } from "@/app/components/ui/button";
import { Pencil, Trash2, BookOpen } from "lucide-react";

interface CampaignCardProps {
  campaign: Campaign;
  onEdit: (campaign: Campaign) => void;
  onDelete: (id: number) => void;
}

export function CampaignCard({ campaign, onEdit, onDelete }: CampaignCardProps) {
  return (
    <Card className="group hover:shadow-xl transition-all duration-300 bg-gradient-to-br from-card to-secondary/30 border-primary/30 hover:border-primary/50">
      <CardHeader className="pb-3">
        <div className="flex items-start justify-between gap-4">
          <div className="flex-1 min-w-0">
            <CardTitle className="text-2xl mb-2 bg-gradient-to-r from-primary to-accent bg-clip-text text-transparent truncate">
              {campaign.name}
            </CardTitle>
            {campaign.gameSystemName && (
              <div className="flex items-center gap-2 text-sm text-muted-foreground">
                <BookOpen className="size-4" />
                <span>{campaign.gameSystemName}</span>
              </div>
            )}
          </div>
        </div>
      </CardHeader>
      <CardContent>
        {campaign.description && (
          <CardDescription className="text-base mb-4 line-clamp-3">
            {campaign.description}
          </CardDescription>
        )}
        
        <div className="flex gap-2 mt-4 pt-4 border-t border-primary/20">
          <Button
            variant="outline"
            size="sm"
            onClick={() => onEdit(campaign)}
            className="flex-1 hover:bg-primary/10 hover:border-primary/50"
          >
            <Pencil className="size-4 mr-2" />
            Edit
          </Button>
          <Button
            variant="outline"
            size="sm"
            onClick={() => onDelete(campaign.id)}
            className="hover:bg-destructive/10 hover:border-destructive/50 hover:text-destructive"
            aria-label="Delete campaign"
            title="Delete campaign"
          >
            <Trash2 className="size-4" />
          </Button>
        </div>
      </CardContent>
    </Card>
  );
}
