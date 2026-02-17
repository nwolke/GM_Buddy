import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@/test/utils'
import { PCCard } from '@/app/components/PCCard'
import { mockPCs } from '@/test/mockData'
import userEvent from '@testing-library/user-event'

describe('PCCard', () => {
  const mockOnEdit = vi.fn()
  const mockOnDelete = vi.fn()

  const defaultProps = {
    pc: mockPCs[0],
    onEdit: mockOnEdit,
    onDelete: mockOnDelete,
  }

  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('should render PC name correctly', () => {
    render(<PCCard {...defaultProps} />)

    expect(screen.getByText(mockPCs[0].name)).toBeInTheDocument()
  })

  it('should render description when present', () => {
    render(<PCCard {...defaultProps} />)

    expect(screen.getByText('Description')).toBeInTheDocument()
    expect(screen.getByText(mockPCs[0].description!)).toBeInTheDocument()
  })

  it('should not render description section when description is absent', () => {
    const pcWithoutDescription = { ...mockPCs[0], description: undefined }
    render(<PCCard {...defaultProps} pc={pcWithoutDescription} />)

    expect(screen.queryByText('Description')).not.toBeInTheDocument()
  })

  it('should call onEdit when edit button is clicked', async () => {
    const user = userEvent.setup()
    render(<PCCard {...defaultProps} />)

    const editButton = screen.getByRole('button', { name: 'Edit PC' })
    expect(editButton).toBeInTheDocument()

    await user.click(editButton)
    expect(mockOnEdit).toHaveBeenCalledWith(mockPCs[0])
  })

  it('should call onDelete with correct id when delete button is clicked', async () => {
    const user = userEvent.setup()
    render(<PCCard {...defaultProps} />)

    const deleteButton = screen.getByRole('button', { name: 'Delete PC' })
    expect(deleteButton).toBeInTheDocument()

    await user.click(deleteButton)
    expect(mockOnDelete).toHaveBeenCalledWith(mockPCs[0].id)
  })

})
