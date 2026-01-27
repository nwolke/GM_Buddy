import { useEffect, useRef, useMemo } from "react";
import ForceGraph2D from "react-force-graph-2d";
import { NPC, Relationship, RelationshipType } from "@/types/npc";
import { Users } from "lucide-react";

interface NPCNetworkProps {
  npcs: NPC[];
  relationships: Relationship[];
  onNodeClick?: (npc: NPC) => void;
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

export function NPCNetwork({ npcs, relationships, onNodeClick }: NPCNetworkProps) {
  const graphRef = useRef<any>();

  const graphData = useMemo(() => {
    const nodes = npcs.map(npc => ({
      id: npc.id,
      name: npc.name,
      race: npc.race,
      class: npc.class,
      npc: npc
    }));

    const links = relationships.map(rel => ({
      source: rel.npcId1,
      target: rel.npcId2,
      type: rel.type,
      description: rel.description,
      color: relationshipColors[rel.type]
    }));

    return { nodes, links };
  }, [npcs, relationships]);

  useEffect(() => {
    if (graphRef.current) {
      graphRef.current.d3Force('charge').strength(-400);
      graphRef.current.d3Force('link').distance(100);
    }
  }, []);

  if (npcs.length === 0) {
    return (
      <div className="flex items-center justify-center h-96 bg-gradient-to-br from-card to-secondary/30 rounded-2xl border border-primary/30">
        <div className="text-center p-8">
          <Users className="size-16 mx-auto mb-4 text-primary/50" />
          <p className="text-muted-foreground text-lg">No characters to visualize yet.</p>
          <p className="text-sm text-muted-foreground mt-2">Recruit some NPCs to see the web!</p>
        </div>
      </div>
    );
  }

  if (relationships.length === 0) {
    return (
      <div className="flex items-center justify-center h-96 bg-gradient-to-br from-card to-secondary/30 rounded-2xl border border-primary/30">
        <div className="text-center p-8">
          <Users className="size-16 mx-auto mb-4 text-accent/50" />
          <p className="text-muted-foreground text-lg">No relationships defined yet.</p>
          <p className="text-sm text-muted-foreground mt-2">Add connections to weave the web of fate!</p>
        </div>
      </div>
    );
  }

  return (
    <div className="border border-primary/30 rounded-2xl overflow-hidden shadow-2xl shadow-primary/10 bg-gradient-to-br from-card to-secondary/20">
      <ForceGraph2D
        ref={graphRef}
        graphData={graphData}
        nodeLabel={(node: any) => `
          <div style="background: linear-gradient(135deg, #1a1333 0%, #2d1b4e 100%); padding: 12px; border-radius: 8px; box-shadow: 0 4px 12px rgba(157, 78, 221, 0.3); border: 1px solid rgba(157, 78, 221, 0.3);">
            <strong style="color: #9d4edd; font-size: 14px;">${node.name}</strong><br/>
            <span style="color: #c77dff; font-size: 12px;">${node.race}</span>
            <span style="color: #a099b8;"> • </span>
            <span style="color: #e0aaff; font-size: 12px;">${node.class}</span>
          </div>
        `}
        nodeCanvasObject={(node: any, ctx: CanvasRenderingContext2D, globalScale: number) => {
          const label = node.name;
          const fontSize = 14 / globalScale;
          ctx.font = `bold ${fontSize}px Sans-Serif`;
          const textWidth = ctx.measureText(label).width;
          const bckgDimensions = [textWidth, fontSize].map(n => n + fontSize * 0.6);

          // Draw outer glow
          const gradient = ctx.createRadialGradient(node.x, node.y, 0, node.x, node.y, 10);
          gradient.addColorStop(0, 'rgba(157, 78, 221, 0.6)');
          gradient.addColorStop(1, 'rgba(157, 78, 221, 0)');
          ctx.fillStyle = gradient;
          ctx.beginPath();
          ctx.arc(node.x, node.y, 10, 0, 2 * Math.PI, false);
          ctx.fill();

          // Draw node circle with gradient
          const nodeGradient = ctx.createRadialGradient(node.x, node.y, 0, node.x, node.y, 6);
          nodeGradient.addColorStop(0, '#c77dff');
          nodeGradient.addColorStop(1, '#9d4edd');
          ctx.fillStyle = nodeGradient;
          ctx.beginPath();
          ctx.arc(node.x, node.y, 6, 0, 2 * Math.PI, false);
          ctx.fill();

          // Draw border
          ctx.strokeStyle = '#e0aaff';
          ctx.lineWidth = 2 / globalScale;
          ctx.stroke();

          // Draw label background with gradient
          const labelY = node.y + 14;
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

          // Draw label border
          ctx.strokeStyle = 'rgba(157, 78, 221, 0.5)';
          ctx.lineWidth = 1 / globalScale;
          ctx.strokeRect(
            node.x - bckgDimensions[0] / 2,
            labelY,
            bckgDimensions[0],
            bckgDimensions[1]
          );

          // Draw label text with glow
          ctx.shadowColor = '#9d4edd';
          ctx.shadowBlur = 4;
          ctx.textAlign = 'center';
          ctx.textBaseline = 'middle';
          ctx.fillStyle = '#e0aaff';
          ctx.fillText(label, node.x, labelY + bckgDimensions[1] / 2);
          ctx.shadowBlur = 0;
        }}
        linkColor={(link: any) => link.color}
        linkWidth={3}
        linkDirectionalParticles={3}
        linkDirectionalParticleWidth={3}
        linkDirectionalParticleSpeed={0.005}
        onNodeClick={(node: any) => {
          if (onNodeClick && node.npc) {
            onNodeClick(node.npc);
          }
        }}
        width={1000}
        height={600}
        backgroundColor="#0f0a1e"
      />
    </div>
  );
}