import '@testing-library/jest-dom'
import { cleanup } from '@testing-library/react'
import { afterEach, vi } from 'vitest'

// Cleanup after each test
afterEach(() => {
  cleanup()
})

// Mock window.matchMedia
Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: vi.fn().mockImplementation(query => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: vi.fn(),
    removeListener: vi.fn(),
    addEventListener: vi.fn(),
    removeEventListener: vi.fn(),
    dispatchEvent: vi.fn(),
  })),
})

// Mock localStorage / sessionStorage with in-memory implementation
const createStorageMock = () => {
  const store = new Map<string, string>()

  return {
    getItem: vi.fn((key: string): string | null => {
      return store.has(key) ? store.get(key)! : null
    }),
    setItem: vi.fn((key: string, value: string): void => {
      store.set(key, String(value))
    }),
    removeItem: vi.fn((key: string): void => {
      store.delete(key)
    }),
    clear: vi.fn((): void => {
      store.clear()
    }),
  }
}

const localStorageMock = createStorageMock()
global.localStorage = localStorageMock as any

// Mock sessionStorage
const sessionStorageMock = createStorageMock()
global.sessionStorage = sessionStorageMock as any
