import { describe, it, expect } from 'vitest'
import { cn } from '@/app/components/ui/utils'

describe('cn utility', () => {
  it('should merge class names correctly', () => {
    const result = cn('px-4', 'py-2')
    expect(result).toBe('px-4 py-2')
  })

  it('should handle conditional classes', () => {
    const isActive = true
    const result = cn('base-class', isActive && 'active-class')
    expect(result).toContain('base-class')
    expect(result).toContain('active-class')
  })

  it('should handle false conditional classes', () => {
    const isActive = false
    const result = cn('base-class', isActive && 'active-class')
    expect(result).toBe('base-class')
    expect(result).not.toContain('active-class')
  })

  it('should merge conflicting Tailwind classes correctly', () => {
    // twMerge should keep the last class when there's a conflict
    const result = cn('px-2', 'px-4')
    expect(result).toBe('px-4')
  })

  it('should handle arrays of classes', () => {
    const result = cn(['px-4', 'py-2'], 'bg-blue-500')
    expect(result).toContain('px-4')
    expect(result).toContain('py-2')
    expect(result).toContain('bg-blue-500')
  })

  it('should handle objects with boolean values', () => {
    const result = cn({
      'px-4': true,
      'py-2': true,
      'hidden': false,
    })
    expect(result).toContain('px-4')
    expect(result).toContain('py-2')
    expect(result).not.toContain('hidden')
  })

  it('should handle undefined and null values', () => {
    const result = cn('px-4', undefined, null, 'py-2')
    expect(result).toBe('px-4 py-2')
  })

  it('should handle empty input', () => {
    const result = cn()
    expect(result).toBe('')
  })
})
