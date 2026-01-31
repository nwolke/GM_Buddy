import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { renderHook, waitFor } from '@testing-library/react'
import { useCampaignData } from '@/hooks/useCampaignData'
import { campaignApi } from '@/services/api'
import { mockCampaigns } from '@/test/mockData'

// Mock the API
vi.mock('@/services/api', () => ({
  campaignApi: {
    getCampaignsByAccount: vi.fn(),
    createCampaign: vi.fn(),
    updateCampaign: vi.fn(),
    deleteCampaign: vi.fn(),
  },
}))

// Mock the auth context
vi.mock('@/contexts/AuthContext', () => ({
  useAuth: vi.fn(),
}))

describe('useCampaignData', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    localStorage.clear()
    
    // Mock useAuth to return authenticated state
    const { useAuth } = require('@/contexts/AuthContext')
    useAuth.mockReturnValue({
      isAuthenticated: true,
      user: { id: 1, email: 'test@test.com' },
    })
  })
  
  afterEach(() => {
    vi.clearAllMocks()
  })

  it('should load campaigns when authenticated', async () => {
    vi.mocked(campaignApi.getCampaignsByAccount).mockResolvedValue(mockCampaigns)

    const { result } = renderHook(() => useCampaignData())

    expect(result.current.loading).toBe(true)

    await waitFor(() => {
      expect(result.current.loading).toBe(false)
    })

    expect(result.current.campaigns).toEqual(mockCampaigns)
    expect(result.current.error).toBeNull()
    expect(campaignApi.getCampaignsByAccount).toHaveBeenCalledTimes(1)
  })

  it('should not load campaigns when not authenticated', async () => {
    const { useAuth } = require('@/contexts/AuthContext')
    useAuth.mockReturnValue({
      isAuthenticated: false,
      user: null,
    })

    const { result } = renderHook(() => useCampaignData())

    await waitFor(() => {
      expect(result.current.loading).toBe(false)
    })

    expect(result.current.campaigns).toEqual([])
    expect(campaignApi.getCampaignsByAccount).not.toHaveBeenCalled()
  })

  it('should handle API errors and fallback to localStorage', async () => {
    const localStorageData = [mockCampaigns[0]]
    localStorage.setItem('ttrpg-campaigns', JSON.stringify(localStorageData))
    
    vi.mocked(campaignApi.getCampaignsByAccount).mockRejectedValue(
      new Error('Network error')
    )

    const { result } = renderHook(() => useCampaignData())

    await waitFor(() => {
      expect(result.current.loading).toBe(false)
    })

    expect(result.current.campaigns).toEqual(localStorageData)
    expect(result.current.error).toContain('Failed to load campaigns')
  })

  it('should save campaigns to localStorage after loading', async () => {
    vi.mocked(campaignApi.getCampaignsByAccount).mockResolvedValue(mockCampaigns)

    renderHook(() => useCampaignData())

    await waitFor(() => {
      const stored = localStorage.getItem('ttrpg-campaigns')
      expect(stored).toBeTruthy()
      expect(JSON.parse(stored!)).toEqual(mockCampaigns)
    })
  })

  it('should create a new campaign', async () => {
    vi.mocked(campaignApi.getCampaignsByAccount).mockResolvedValue(mockCampaigns)
    const newCampaign = {
      id: 3,
      name: 'New Campaign',
      description: 'A new test campaign',
      gameSystemId: 1,
      gameSystemName: 'D&D 5e',
      accountId: 1,
    }
    vi.mocked(campaignApi.createCampaign).mockResolvedValue(newCampaign)

    const { result } = renderHook(() => useCampaignData())

    await waitFor(() => {
      expect(result.current.loading).toBe(false)
    })

    const campaignData = {
      name: 'New Campaign',
      description: 'A new test campaign',
      gameSystemId: 1,
      gameSystemName: 'D&D 5e',
      accountId: 1,
    }

    await result.current.saveCampaign(campaignData)

    expect(campaignApi.createCampaign).toHaveBeenCalledWith({
      name: campaignData.name,
      description: campaignData.description,
      game_system_id: campaignData.gameSystemId,
    })
  })

  it('should update an existing campaign', async () => {
    vi.mocked(campaignApi.getCampaignsByAccount).mockResolvedValue(mockCampaigns)
    vi.mocked(campaignApi.updateCampaign).mockResolvedValue(undefined)

    const { result } = renderHook(() => useCampaignData())

    await waitFor(() => {
      expect(result.current.loading).toBe(false)
    })

    const updatedCampaign = {
      ...mockCampaigns[0],
      name: 'Updated Campaign Name',
    }

    await result.current.saveCampaign(updatedCampaign)

    expect(campaignApi.updateCampaign).toHaveBeenCalledWith(
      updatedCampaign.id,
      {
        name: updatedCampaign.name,
        description: updatedCampaign.description,
        game_system_id: updatedCampaign.gameSystemId,
      }
    )
  })

  it('should delete a campaign', async () => {
    vi.mocked(campaignApi.getCampaignsByAccount).mockResolvedValue(mockCampaigns)
    vi.mocked(campaignApi.deleteCampaign).mockResolvedValue(undefined)

    const { result } = renderHook(() => useCampaignData())

    await waitFor(() => {
      expect(result.current.loading).toBe(false)
    })

    const initialCount = result.current.campaigns.length

    await result.current.deleteCampaign(mockCampaigns[0].id)

    expect(campaignApi.deleteCampaign).toHaveBeenCalledWith(mockCampaigns[0].id)
    expect(result.current.campaigns.length).toBe(initialCount - 1)
    expect(result.current.campaigns.find(c => c.id === mockCampaigns[0].id)).toBeUndefined()
  })

  it('should refresh campaigns', async () => {
    vi.mocked(campaignApi.getCampaignsByAccount).mockResolvedValue(mockCampaigns)

    const { result } = renderHook(() => useCampaignData())

    await waitFor(() => {
      expect(result.current.loading).toBe(false)
    })

    // Clear the mock to ensure it's called again
    vi.mocked(campaignApi.getCampaignsByAccount).mockClear()
    vi.mocked(campaignApi.getCampaignsByAccount).mockResolvedValue([...mockCampaigns, {
      id: 3,
      name: 'Refreshed Campaign',
      description: 'New campaign',
      gameSystemId: 1,
      gameSystemName: 'D&D 5e',
      accountId: 1,
    }])

    await result.current.refreshCampaigns()

    expect(campaignApi.getCampaignsByAccount).toHaveBeenCalledTimes(1)
  })
})
