import { useEffect, useRef, useMemo } from "react";
import ForceGraph2D from "react-force-graph-2d";
import { Relationship, RelationshipType } from "@/types/npc";
import { EntityItem } from "@/hooks/useRelationshipPageData";
import { Users } from "lucide-react";

interface EntityGraphProps {
  entities: EntityItem[];
  relationships: Relationship[];
  selectedEntityId?: number | null;
  selectedEntityType?: 'npc' | 'pc' | null;
  onNodeClick?: (entity: EntityItem) => void;
  width?: number;
  height?: number;
}

const relationshipColors: Record<RelationshipType, string> = {
  ally: '#10b981',
  enemy: '#ef4444',
  family: '#a855f7',
  rival: '#f97316',
  mentor: '#3b82f6',
  student: '#06b6d4',
  neutral: '#6b7280'
};

// Node colors by entity type
const NPC_NODE_COLOR_INNER = '#c77dff';
const NPC_NODE_COLOR_OUTER = '#9d4edd';
const NPC_GLOW_COLOR = 'rgba(157, 78, 221, 0.6)';
const NPC_BORDER_COLOR = '#e0aaff';
const NPC_LABEL_COLOR = '#e0aaff';

const PC_NODE_COLOR_INNER = '#4ade80';
const PC_NODE_COLOR_OUTER = '#22c55e';
const PC_GLOW_COLOR = 'rgba(34, 197, 94, 0.6)';
const PC_BORDER_COLOR = '#86efac';
const PC_LABEL_COLOR = '#86efac';

export function EntityGraph({
  entities,
  relationships,
  selectedEntityId,
  selectedEntityType,
  onNodeClick,
  width = 800,
  height = 600,
}: EntityGraphProps) {
  const graphRef = useRef<any>();

  const graphData = useMemo(() => {
    const nodes = entities.map(entity => ({
      id: `${entity.entityType}-${entity.id}`,
      entityId: entity.id,
      entityType: entity.entityType,
      name: entity.name,
      subtitle: entity.entityType === 'npc'
        ? [entity.race, entity.class].filter(Boolean).join(' • ')
        : 'Player Character',
      entity,
    }));

    const links = relationships.map(rel => ({
      source: `${rel.entityType1}-${rel.npcId1}`,
      target: `${rel.entityType2}-${rel.npcId2}`,
      type: rel.type,
      description: rel.description,
      color: relationshipColors[rel.type] ?? relationshipColors.neutral,
    }));

    return { nodes, links };
  }, [entities, relationships]);

  useEffect(() => {
    if (graphRef.current) {
      graphRef.current.d3Force('charge').strength(-400);
      graphRef.current.d3Force('link').distance(100);
    }
  }, []);

  if (entities.length === 0) {
    return (
      <div className="flex items-center justify-center h-full bg-gradient-to-br from-card to-secondary/30 rounded-2xl border border-primary/30">
        <div className="text-center p-8">
          <Users className="size-16 mx-auto mb-4 text-primary/50" />
          <p className="text-muted-foreground text-lg">No entities to visualize yet.</p>
          <p className="text-sm text-muted-foreground mt-2">Add NPCs and PCs to see the web!</p>
        </div>
      </div>
    );
  }

  if (relationships.length === 0) {
    return (
      <div className="flex items-center justify-center h-full bg-gradient-to-br from-card to-secondary/30 rounded-2xl border border-primary/30">
        <div className="text-center p-8">
          <Users className="size-16 mx-auto mb-4 text-accent/50" />
          <p className="text-muted-foreground text-lg">No relationships defined yet.</p>
          <p className="text-sm text-muted-foreground mt-2">Select an entity and add connections!</p>
        </div>
      </div>
    );
  }

  const isSelected = (node: any) =>
    selectedEntityId != null &&
    node.entityId === selectedEntityId &&
    node.entityType === selectedEntityType;

  return (
    <div className="border border-primary/30 rounded-2xl overflow-hidden shadow-2xl shadow-primary/10 bg-gradient-to-br from-card to-secondary/20 h-full">
      <ForceGraph2D
        ref={graphRef}
        graphData={graphData}
        nodeLabel={(node: any) => `
          <div style="background: linear-gradient(135deg, #1a1333 0%, #2d1b4e 100%); padding: 12px; border-radius: 8px; box-shadow: 0 4px 12px rgba(157, 78, 221, 0.3); border: 1px solid rgba(157, 78, 221, 0.3);">
            <strong style="color: ${node.entityType === 'pc' ? '#4ade80' : '#9d4edd'}; font-size: 14px;">${node.name}</strong><br/>
            <span style="color: #a099b8; font-size: 12px;">${node.subtitle}</span>
          </div>
        `}
        nodeCanvasObject={(node: any, ctx: CanvasRenderingContext2D, globalScale: number) => {
          // Guard against non-finite coordinates during early simulation ticks
          if (!Number.isFinite(node.x) || !Number.isFinite(node.y)) return;

          const isNpc = node.entityType === 'npc';
          const selected = isSelected(node);
          const innerColor = isNpc ? NPC_NODE_COLOR_INNER : PC_NODE_COLOR_INNER;
          const outerColor = isNpc ? NPC_NODE_COLOR_OUTER : PC_NODE_COLOR_OUTER;
          const glowColor = isNpc ? NPC_GLOW_COLOR : PC_GLOW_COLOR;
          const borderColor = isNpc ? NPC_BORDER_COLOR : PC_BORDER_COLOR;
          const labelColor = isNpc ? NPC_LABEL_COLOR : PC_LABEL_COLOR;

          const label = node.name;
          const fontSize = 14 / globalScale;
          ctx.font = `bold ${fontSize}px Sans-Serif`;
          const textWidth = ctx.measureText(label).width;
          const bckgDimensions = [textWidth, fontSize].map(n => n + fontSize * 0.6);

          const nodeRadius = selected ? 9 : 6;
          const glowRadius = selected ? 14 : 10;

          // Draw outer glow
          const gradient = ctx.createRadialGradient(node.x, node.y, 0, node.x, node.y, glowRadius);
          gradient.addColorStop(0, glowColor);
          gradient.addColorStop(1, 'rgba(0,0,0,0)');
          ctx.fillStyle = gradient;
          ctx.beginPath();
          ctx.arc(node.x, node.y, glowRadius, 0, 2 * Math.PI, false);
          ctx.fill();

          // Draw node circle
          const nodeGradient = ctx.createRadialGradient(node.x, node.y, 0, node.x, node.y, nodeRadius);
          nodeGradient.addColorStop(0, innerColor);
          nodeGradient.addColorStop(1, outerColor);
          ctx.fillStyle = nodeGradient;
          ctx.beginPath();
          ctx.arc(node.x, node.y, nodeRadius, 0, 2 * Math.PI, false);
          ctx.fill();

          // Draw border (thicker when selected)
          ctx.strokeStyle = selected ? '#ffffff' : borderColor;
          ctx.lineWidth = selected ? 3 / globalScale : 2 / globalScale;
          ctx.stroke();

          // Draw label background
          const labelY = node.y + nodeRadius + 6;
          const bgGradient = ctx.createLinearGradient(
            node.x - bckgDimensions[0] / 2,
            labelY,
            node.x + bckgDimensions[0] / 2,
            labelY + bckgDimensions[1]
          );
          bgGradient.addColorStop(0, 'rgba(26, 19, 51, 0.95)');
          bgGradient.addColorStop(1, 'rgba(45, 27, 78, 0.95)');
          ctx.fillStyle = bgGradient;
          ctx.fillRect(
            node.x - bckgDimensions[0] / 2,
            labelY,
            bckgDimensions[0],
            bckgDimensions[1]
          );

          // Label border
          ctx.strokeStyle = selected ? outerColor : 'rgba(157, 78, 221, 0.5)';
          ctx.lineWidth = selected ? 1.5 / globalScale : 1 / globalScale;
          ctx.strokeRect(
            node.x - bckgDimensions[0] / 2,
            labelY,
            bckgDimensions[0],
            bckgDimensions[1]
          );

          // Label text
          ctx.shadowColor = outerColor;
          ctx.shadowBlur = 4;
          ctx.textAlign = 'center';
          ctx.textBaseline = 'middle';
          ctx.fillStyle = labelColor;
          ctx.fillText(label, node.x, labelY + bckgDimensions[1] / 2);
          ctx.shadowBlur = 0;
        }}
        linkColor={(link: any) => link.color}
        linkWidth={3}
        linkDirectionalParticles={3}
        linkDirectionalParticleWidth={3}
        linkDirectionalParticleSpeed={0.005}
        onNodeClick={(node: any) => {
          if (onNodeClick && node.entity) {
            onNodeClick(node.entity);
          }
        }}
        width={width}
        height={height}
        backgroundColor="#0f0a1e"
      />
    </div>
  );
}
