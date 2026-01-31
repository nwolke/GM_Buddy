import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@/test/utils'
import { NPCCard } from '@/app/components/NPCCard'
import { mockNPCs } from '@/test/mockData'
import userEvent from '@testing-library/user-event'

describe('NPCCard', () => {
  const mockOnEdit = vi.fn()
  const mockOnDelete = vi.fn()
  const mockOnManageRelationships = vi.fn()

  const defaultProps = {
    npc: mockNPCs[0],
    onEdit: mockOnEdit,
    onDelete: mockOnDelete,
    onManageRelationships: mockOnManageRelationships,
    relationshipCount: 0,
  }

  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('should render NPC information correctly', () => {
    render(<NPCCard {...defaultProps} />)

    expect(screen.getByText(mockNPCs[0].name)).toBeInTheDocument()
    expect(screen.getByText(mockNPCs[0].race)).toBeInTheDocument()
    expect(screen.getByText(mockNPCs[0].class)).toBeInTheDocument()
    expect(screen.getByText(mockNPCs[0].description)).toBeInTheDocument()
  })

  it('should render faction when present', () => {
    render(<NPCCard {...defaultProps} />)

    expect(screen.getByText('Faction')).toBeInTheDocument()
    expect(screen.getByText(mockNPCs[0].faction!)).toBeInTheDocument()
  })

  it('should render notes when present', () => {
    render(<NPCCard {...defaultProps} />)

    expect(screen.getByText('Notes')).toBeInTheDocument()
    expect(screen.getByText(mockNPCs[0].notes!)).toBeInTheDocument()
  })

  it('should not render faction section when faction is not present', () => {
    const npcWithoutFaction = { ...mockNPCs[0], faction: undefined }
    render(<NPCCard {...defaultProps} npc={npcWithoutFaction} />)

    expect(screen.queryByText('Faction')).not.toBeInTheDocument()
  })

  it('should not render notes section when notes are not present', () => {
    const npcWithoutNotes = { ...mockNPCs[0], notes: undefined }
    render(<NPCCard {...defaultProps} npc={npcWithoutNotes} />)

    expect(screen.queryByText('Notes')).not.toBeInTheDocument()
  })

  it('should display relationship count when greater than 0', () => {
    render(<NPCCard {...defaultProps} relationshipCount={3} />)

    expect(screen.getByText('3 connections')).toBeInTheDocument()
  })

  it('should display singular "connection" when count is 1', () => {
    render(<NPCCard {...defaultProps} relationshipCount={1} />)

    expect(screen.getByText('1 connection')).toBeInTheDocument()
  })

  it('should not display relationship count when 0', () => {
    render(<NPCCard {...defaultProps} relationshipCount={0} />)

    expect(screen.queryByText(/connection/)).not.toBeInTheDocument()
  })

  it('should call onEdit when edit button is clicked', async () => {
    const user = userEvent.setup()
    render(<NPCCard {...defaultProps} />)

    const editButton = screen.getByRole('button', { name: '' }).parentElement?.querySelector('[data-lucide="pencil"]')?.closest('button')
    expect(editButton).toBeInTheDocument()
    
    if (editButton) {
      await user.click(editButton)
      expect(mockOnEdit).toHaveBeenCalledWith(mockNPCs[0])
    }
  })

  it('should call onDelete when delete button is clicked', async () => {
    const user = userEvent.setup()
    render(<NPCCard {...defaultProps} />)

    const deleteButton = screen.getByRole('button', { name: '' }).parentElement?.querySelector('[data-lucide="trash-2"]')?.closest('button')
    expect(deleteButton).toBeInTheDocument()
    
    if (deleteButton) {
      await user.click(deleteButton)
      expect(mockOnDelete).toHaveBeenCalledWith(mockNPCs[0].id)
    }
  })

  it('should call onManageRelationships when relationships button is clicked', async () => {
    const user = userEvent.setup()
    render(<NPCCard {...defaultProps} />)

    const relationshipsButton = screen.getByRole('button', { name: '' }).parentElement?.querySelector('[data-lucide="users"]')?.closest('button')
    expect(relationshipsButton).toBeInTheDocument()
    
    if (relationshipsButton) {
      await user.click(relationshipsButton)
      expect(mockOnManageRelationships).toHaveBeenCalledWith(mockNPCs[0])
    }
  })
})
