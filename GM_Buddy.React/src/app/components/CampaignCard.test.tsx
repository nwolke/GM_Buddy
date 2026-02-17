import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@/test/utils'
import { CampaignCard } from '@/app/components/CampaignCard'
import { mockCampaigns } from '@/test/mockData'
import userEvent from '@testing-library/user-event'

describe('CampaignCard', () => {
  const mockOnEdit = vi.fn()
  const mockOnDelete = vi.fn()

  const defaultProps = {
    campaign: mockCampaigns[0],
    onEdit: mockOnEdit,
    onDelete: mockOnDelete,
  }

  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('should render campaign information correctly', () => {
    render(<CampaignCard {...defaultProps} />)

    expect(screen.getByText(mockCampaigns[0].name)).toBeInTheDocument()
    expect(screen.getByText(mockCampaigns[0].description!)).toBeInTheDocument()
  })

  it('should render without description when not provided', () => {
    const campaignWithoutDescription = { ...mockCampaigns[0], description: undefined }
    render(<CampaignCard {...defaultProps} campaign={campaignWithoutDescription} />)

    expect(screen.getByText(mockCampaigns[0].name)).toBeInTheDocument()
    expect(screen.queryByText(mockCampaigns[0].description!)).not.toBeInTheDocument()
  })

  it('should call onEdit when edit button is clicked', async () => {
    const user = userEvent.setup()
    render(<CampaignCard {...defaultProps} />)

    const editButton = screen.getByRole('button', { name: /edit/i })
    await user.click(editButton)

    expect(mockOnEdit).toHaveBeenCalledWith(mockCampaigns[0])
    expect(mockOnEdit).toHaveBeenCalledTimes(1)
  })

  it('should call onDelete when delete button is clicked', async () => {
    const user = userEvent.setup()
    render(<CampaignCard {...defaultProps} />)

    const deleteButton = screen.getByRole('button', { name: /delete campaign/i })
    await user.click(deleteButton)

    expect(mockOnDelete).toHaveBeenCalledWith(mockCampaigns[0].id)
    expect(mockOnDelete).toHaveBeenCalledTimes(1)
  })

  it('should have proper styling classes applied', () => {
    const { container } = render(<CampaignCard {...defaultProps} />)

    const card = container.querySelector('.group')
    expect(card).toBeInTheDocument()
    expect(card).toHaveClass('hover:shadow-xl')
  })
})
