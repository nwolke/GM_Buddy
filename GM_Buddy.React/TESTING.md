# Front-End Testing Guide

This document provides information about the front-end test suite for the GM_Buddy React application.

## Test Infrastructure

### Tools & Libraries
- **Vitest**: Fast unit test framework that integrates seamlessly with Vite
- **React Testing Library**: Testing utilities for React components
- **@testing-library/jest-dom**: Custom matchers for DOM assertions
- **@testing-library/user-event**: Utilities for simulating user interactions  
- **jsdom**: DOM implementation for Node.js used by Vitest
- **MSW (Mock Service Worker)**: API mocking library (available for future use)

### Configuration

Test configuration is located in `vite.config.ts`:
```typescript
test: {
  globals: true,
  environment: 'jsdom',
  setupFiles: './src/test/setup.ts',
  css: true,
  coverage: {
    provider: 'v8',
    reporter: ['text', 'json', 'html'],
  },
}
```

### Test Setup

Global test setup is in `src/test/setup.ts`, which:
- Imports `@testing-library/jest-dom` matchers
- Cleans up after each test
- Mocks `window.matchMedia`
- Mocks localStorage and sessionStorage

### Test Utilities

**`src/test/utils.tsx`**: Provides a custom `renderWithProviders` function that wraps components with necessary providers (AuthProvider, BrowserRouter).

**`src/test/mockData.ts`**: Contains mock data for campaigns, NPCs, and relationships used across tests.

## Running Tests

```bash
# Run tests in watch mode
npm test

# Run tests once
npm test -- --run

# Run tests with UI
npm test:ui

# Run tests with coverage
npm test:coverage
```

## Test Coverage

### Custom Hooks (src/hooks/)

#### useCampaignData.test.ts
Tests for the campaign data management hook:
- Loading campaigns when authenticated
- Not loading when unauthenticated
- API error handling with localStorage fallback
- Creating new campaigns
- Updating existing campaigns
- Deleting campaigns
- Refreshing campaign data
- Saving to localStorage

#### useNPCData.test.ts  
Tests for the NPC and relationship management hook:
- Loading NPCs and relationships when authenticated
- Filtering NPCs by campaign
- Filtering relationships to match campaign NPCs
- Not loading when unauthenticated
- API error handling with localStorage fallback
- Creating new NPCs
- Updating existing NPCs
- Deleting NPCs
- Managing NPC relationships
- Campaign change handling

### Components (src/app/components/)

#### NPCCard.test.tsx
Tests for the NPC card component:
- Rendering NPC information (name, race, class, description)
- Conditional rendering of faction and notes
- Displaying relationship counts
- Button click handlers (edit, delete, manage relationships)

#### CampaignCard.test.tsx
Tests for the campaign card component:
- Rendering campaign information
- Conditional rendering of description and game system
- Button click handlers (edit, delete)
- Styling classes

### Contexts (src/contexts/)

#### AuthContext.test.tsx
Tests for the authentication context:
- Providing auth context to components
- Error handling when used outside provider
- Restoring session from localStorage
- Handling invalid stored auth data
- Cognito callback handling
- Login and logout functionality
- Cognito mode configuration

## Writing New Tests

### Component Tests

```typescript
import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@/test/utils'
import { MyComponent } from '@/app/components/MyComponent'
import userEvent from '@testing-library/user-event'

describe('MyComponent', () => {
  it('should render correctly', () => {
    render(<MyComponent />)
    expect(screen.getByText('Expected Text')).toBeInTheDocument()
  })

  it('should handle user interaction', async () => {
    const user = userEvent.setup()
    const handleClick = vi.fn()
    
    render(<MyComponent onClick={handleClick} />)
    
    await user.click(screen.getByRole('button'))
    expect(handleClick).toHaveBeenCalled()
  })
})
```

### Hook Tests

```typescript
import { describe, it, expect, vi } from 'vitest'
import { renderHook, waitFor } from '@testing-library/react'
import { useMyHook } from '@/hooks/useMyHook'

// Mock dependencies
vi.mock('@/services/api', () => ({
  myApi: {
    getData: vi.fn(),
  },
}))

describe('useMyHook', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('should load data', async () => {
    const mockData = [{ id: 1, name: 'Test' }]
    vi.mocked(myApi.getData).mockResolvedValue(mockData)

    const { result } = renderHook(() => useMyHook())

    await waitFor(() => {
      expect(result.current.loading).toBe(false)
    })

    expect(result.current.data).toEqual(mockData)
  })
})
```

## Best Practices

1. **Test Behavior, Not Implementation**: Focus on what the component does from a user's perspective
2. **Use Descriptive Test Names**: Test names should clearly describe what is being tested
3. **Arrange-Act-Assert**: Structure tests with clear setup, action, and assertion phases
4. **Clean Up**: Use `beforeEach` and `afterEach` to ensure test isolation
5. **Mock External Dependencies**: Mock API calls, external services, and complex dependencies
6. **Test Edge Cases**: Include tests for error states, empty states, and boundary conditions
7. **Avoid Testing Implementation Details**: Don't test internal state or methods directly

## Common Patterns

### Mocking the Auth Context

```typescript
vi.mock('@/contexts/AuthContext', () => ({
  useAuth: vi.fn(),
}))

// In test
const { useAuth } = require('@/contexts/AuthContext')
useAuth.mockReturnValue({
  isAuthenticated: true,
  user: { id: 1, email: 'test@test.com' },
})
```

### Waiting for Async Operations

```typescript
await waitFor(() => {
  expect(result.current.loading).toBe(false)
})
```

### Testing User Interactions

```typescript
const user = userEvent.setup()
await user.click(screen.getByRole('button', { name: /submit/i }))
await user.type(screen.getByLabelText(/name/i), 'John Doe')
```

### Querying Elements

```typescript
// Preferred: By role (most accessible)
screen.getByRole('button', { name: /submit/i })

// By label text
screen.getByLabelText(/email/i)

// By text content
screen.getByText('Welcome')

// By test ID (when semantic queries aren't possible)
screen.getByTestId('custom-element')
```

## Debugging Tests

### View DOM Structure
```typescript
import { screen } from '@/test/utils'
screen.debug() // Prints entire DOM
screen.debug(screen.getByRole('button')) // Prints specific element
```

### Run Single Test
```bash
npm test -- -t "test name pattern"
```

### Run Single File
```bash
npm test -- src/hooks/useCampaignData.test.ts
```

## Future Enhancements

- [ ] Add MSW handlers for API mocking
- [ ] Add E2E tests with Playwright or Cypress
- [ ] Increase test coverage to 80%+
- [ ] Add visual regression testing
- [ ] Add accessibility testing with axe-core
- [ ] Add performance testing

## Resources

- [Vitest Documentation](https://vitest.dev/)
- [React Testing Library](https://testing-library.com/docs/react-testing-library/intro/)
- [Testing Library Queries](https://testing-library.com/docs/queries/about)
- [Common Testing Mistakes](https://kentcdodds.com/blog/common-mistakes-with-react-testing-library)
