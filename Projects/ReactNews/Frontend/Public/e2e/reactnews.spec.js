import { expect, test } from '@playwright/test'

const apiBaseUrl = process.env.VITE_API_BASE_URL ?? 'http://127.0.0.1:5227'

/*
 * What: Creates a unique email address for each test run.
 * How: combines the current timestamp with a short random string.
 * Why: even though the E2E database is reset at API startup, unique emails make
 * failures easier to debug from logs and protect against accidental database reuse.
 */
function uniqueEmail(prefix) {
  return `${prefix}-${Date.now()}-${Math.random().toString(36).slice(2)}@example.com`
}

/*
 * What: Registers a normal Reader account through the real login page.
 * How: fills the visible account form and clicks Create account.
 * Why: E2E tests should use the same UI path a real user would use instead of
 * calling backend setup endpoints that do not exist in production.
 */
async function registerReader(page, email, displayName = 'E2E Reader') {
  await page.goto('/login')
  await page.getByLabel('Email').fill(email)
  await page.getByLabel('Display name').fill(displayName)
  await page.getByLabel('Password').fill('Password123!')
  await page.getByRole('button', { name: 'Create account' }).click()
  await expect(page.getByText(`Signed in as ${displayName} (Reader).`)).toBeVisible()
}

/*
 * What: Logs in with an existing account through the real login page.
 * How: fills Email and Password, then clicks Sign in.
 * Why: this proves the cookie-auth flow works from browser UI to backend API.
 */
async function login(page, email, password) {
  await page.goto('/login')
  await page.getByLabel('Email').fill(email)
  await page.getByLabel('Password').fill(password)
  await page.getByRole('button', { name: 'Sign in' }).click()
}

/*
 * What: Creates one valid editorial article request body for API health checks.
 * How: returns all fields required by the backend validation rules and allows the
 * title/status/body to be customized by each check.
 * Why: endpoint health checks should only fail for endpoint behavior problems,
 * not because repeated setup objects accidentally miss required fields.
 */
function editorialRequest({
  title = `Health editorial ${Date.now()}`,
  status = 'Draft',
  body = 'This health-check editorial body is long enough to pass validation.'
} = {}) {
  return {
    title,
    summary: 'This editorial article is used by the health-check E2E test.',
    body,
    author: 'Health Admin',
    category: 'technology',
    imageUrl: '',
    status
  }
}

test('guest can open the app shell and public editorial feed', async ({ page }) => {
  await page.goto('/')

  await expect(page.getByRole('heading', { name: 'ReactNews' })).toBeVisible()
  await expect(page.getByRole('link', { name: 'News' })).toBeVisible()

  await page.getByRole('link', { name: 'Editorial' }).click()

  await expect(page.getByRole('heading', { name: 'Editorial Feed' })).toBeVisible()
  await expect(page.getByText('No published editorial articles')).toBeVisible()
})

test('reader can register, edit profile, change password, and delete account', async ({ page }) => {
  const email = uniqueEmail('reader')

  await registerReader(page, email)
  await page.getByRole('link', { name: 'Profile' }).click()
  await expect(page.getByRole('heading', { name: 'Profile' })).toBeVisible()

  const accountPanel = page.locator('.account-panel')
  const displayNameInput = accountPanel.getByLabel('Display name')
  await displayNameInput.fill('Updated E2E Reader')
  await expect(displayNameInput).toHaveValue('Updated E2E Reader')

  await Promise.all([
    page.waitForResponse((response) =>
      response.url().includes('/api/auth/profile') && response.status() === 200),
    accountPanel.getByRole('button', { name: 'Save profile' }).click()
  ])

  await expect(accountPanel.getByText('Updated E2E Reader')).toBeVisible()

  await page.getByLabel('Current password').first().fill('Password123!')
  await page.getByLabel('New password').fill('NewPassword123!')
  await page.getByRole('button', { name: 'Change password' }).click()

  await page.getByLabel('Current password').last().fill('NewPassword123!')
  await page.getByRole('button', { name: 'Delete account' }).click()

  await expect(page.getByRole('link', { name: 'Login' })).toBeVisible()
  await login(page, email, 'NewPassword123!')
  await expect(page.getByText('Email or password is incorrect.')).toBeVisible()
})

test('auth reader admin and public endpoint health matrix', async ({ browser, request }) => {
  /*
   * What: Checks anonymous/public endpoint boundaries before any login happens.
   * How: sends direct API requests to health, public editorial, auth, reader, and
   * admin routes.
   * Why: a health matrix should prove not only success paths but also the
   * expected 401/403/404/400 responses that protect private data.
   */
  const healthResponse = await request.get(`${apiBaseUrl}/api/health`)
  expect(healthResponse.status(), 'GET /api/health').toBe(200)

  const publicEditorialResponse = await request.get(`${apiBaseUrl}/api/public/editorial/articles`)
  expect(publicEditorialResponse.status(), 'GET /api/public/editorial/articles').toBe(200)

  const anonymousSavedResponse = await request.get(`${apiBaseUrl}/api/saved-articles`)
  expect(anonymousSavedResponse.status(), 'anonymous GET /api/saved-articles').toBe(401)

  const anonymousPreferencesResponse = await request.get(`${apiBaseUrl}/api/reader-preferences`)
  expect(anonymousPreferencesResponse.status(), 'anonymous GET /api/reader-preferences').toBe(401)

  const anonymousAdminResponse = await request.get(`${apiBaseUrl}/api/editorial/articles`)
  expect(anonymousAdminResponse.status(), 'anonymous GET /api/editorial/articles').toBe(401)

  const invalidLoginResponse = await request.post(`${apiBaseUrl}/api/auth/login`, {
    data: {
      email: 'missing@example.com',
      password: 'Password123!'
    }
  })
  expect(invalidLoginResponse.status(), 'invalid login').toBe(400)

  /*
   * What: Checks Auth and Reader endpoints with a real Reader browser session.
   * How: registers through the UI so cookies are created normally, then uses the
   * same browser context request client for direct endpoint checks.
   * Why: this proves frontend auth cookies work for both page actions and API
   * calls that require the Reader/Admin role boundary.
   */
  const readerContext = await browser.newContext()
  const readerPage = await readerContext.newPage()
  const readerEmail = uniqueEmail('health-reader')
  await registerReader(readerPage, readerEmail, 'Health Reader')

  const readerApi = readerContext.request
  const meResponse = await readerApi.get(`${apiBaseUrl}/api/auth/me`)
  expect(meResponse.status(), 'reader GET /api/auth/me').toBe(200)

  const duplicateRegisterResponse = await readerApi.post(`${apiBaseUrl}/api/auth/register`, {
    data: {
      email: readerEmail,
      displayName: 'Duplicate Reader',
      password: 'Password123!'
    }
  })
  expect(duplicateRegisterResponse.status(), 'duplicate register').toBe(400)

  const profileResponse = await readerApi.put(`${apiBaseUrl}/api/auth/profile`, {
    data: { displayName: 'Health Reader Updated' }
  })
  expect(profileResponse.status(), 'reader PUT /api/auth/profile').toBe(200)

  const wrongPasswordChangeResponse = await readerApi.put(`${apiBaseUrl}/api/auth/password`, {
    data: {
      currentPassword: 'WrongPassword123!',
      newPassword: 'NewPassword123!'
    }
  })
  expect(wrongPasswordChangeResponse.status(), 'wrong current password').toBe(400)

  const preferencesResponse = await readerApi.get(`${apiBaseUrl}/api/reader-preferences`)
  expect(preferencesResponse.status(), 'reader GET /api/reader-preferences').toBe(200)

  const updatePreferencesResponse = await readerApi.put(`${apiBaseUrl}/api/reader-preferences`, {
    data: {
      theme: 'dark',
      fontScale: 1.15,
      compactCards: true,
      preferredCategories: ['technology', 'sports']
    }
  })
  expect(updatePreferencesResponse.status(), 'reader PUT /api/reader-preferences').toBe(200)

  const invalidPreferencesResponse = await readerApi.put(`${apiBaseUrl}/api/reader-preferences`, {
    data: {
      theme: 'blue',
      fontScale: 1,
      compactCards: false,
      preferredCategories: ['technology']
    }
  })
  expect(invalidPreferencesResponse.status(), 'invalid reader preferences').toBe(400)

  const savedListResponse = await readerApi.get(`${apiBaseUrl}/api/saved-articles`)
  expect(savedListResponse.status(), 'reader GET /api/saved-articles').toBe(200)

  const missingSaveResponse = await readerApi.post(`${apiBaseUrl}/api/saved-articles/missing-snapshot`)
  expect(missingSaveResponse.status(), 'reader POST missing saved article').toBe(404)

  const missingRemoveResponse = await readerApi.delete(`${apiBaseUrl}/api/saved-articles/missing-snapshot`)
  expect(missingRemoveResponse.status(), 'reader DELETE missing saved article').toBe(404)

  const readerAdminResponse = await readerApi.get(`${apiBaseUrl}/api/editorial/articles`)
  expect(readerAdminResponse.status(), 'reader GET /api/editorial/articles').toBe(403)

  const correctPasswordChangeResponse = await readerApi.put(`${apiBaseUrl}/api/auth/password`, {
    data: {
      currentPassword: 'Password123!',
      newPassword: 'NewPassword123!'
    }
  })
  expect(correctPasswordChangeResponse.status(), 'correct password change').toBe(200)

  const readerLogoutResponse = await readerApi.post(`${apiBaseUrl}/api/auth/logout`)
  expect(readerLogoutResponse.status(), 'reader POST /api/auth/logout').toBe(200)

  const readerMeAfterLogoutResponse = await readerApi.get(`${apiBaseUrl}/api/auth/me`)
  expect(readerMeAfterLogoutResponse.status(), 'reader /me after logout').toBe(401)

  await login(readerPage, readerEmail, 'NewPassword123!')
  await expect(readerPage.getByText('Signed in as Health Reader Updated (Reader).')).toBeVisible()

  const wrongDeleteResponse = await readerApi.delete(`${apiBaseUrl}/api/auth/account`, {
    data: { currentPassword: 'WrongPassword123!' }
  })
  expect(wrongDeleteResponse.status(), 'wrong delete password').toBe(400)

  const deleteResponse = await readerApi.delete(`${apiBaseUrl}/api/auth/account`, {
    data: { currentPassword: 'NewPassword123!' }
  })
  expect(deleteResponse.status(), 'reader DELETE /api/auth/account').toBe(200)

  const readerMeAfterDeleteResponse = await readerApi.get(`${apiBaseUrl}/api/auth/me`)
  expect(readerMeAfterDeleteResponse.status(), 'reader /me after delete').toBe(401)
  await readerContext.close()

  /*
   * What: Checks Admin endpoint success, validation, publishing, public reading,
   * and archiving behavior.
   * How: logs in with the seeded Admin, creates/updates/publishes/archives one
   * article, and checks public detail visibility before and after archive.
   * Why: Admin endpoints are the highest-risk local workflow because they change
   * public content.
   */
  const adminContext = await browser.newContext()
  const adminPage = await adminContext.newPage()
  await login(adminPage, 'admin-e2e@example.com', 'Password123!')
  await expect(adminPage.getByText('Signed in as E2E Admin (Admin).')).toBeVisible()
  const adminApi = adminContext.request

  const adminListResponse = await adminApi.get(`${apiBaseUrl}/api/editorial/articles`)
  expect(adminListResponse.status(), 'admin GET /api/editorial/articles').toBe(200)

  const invalidEditorialResponse = await adminApi.post(`${apiBaseUrl}/api/editorial/articles`, {
    data: editorialRequest({ title: 'Bad' })
  })
  expect(invalidEditorialResponse.status(), 'invalid editorial create').toBe(400)

  const createResponse = await adminApi.post(`${apiBaseUrl}/api/editorial/articles`, {
    data: editorialRequest()
  })
  expect(createResponse.status(), 'admin POST /api/editorial/articles').toBe(200)
  const created = await createResponse.json()

  const getCreatedResponse = await adminApi.get(`${apiBaseUrl}/api/editorial/articles/${created.id}`)
  expect(getCreatedResponse.status(), 'admin GET /api/editorial/articles/{id}').toBe(200)

  const updateResponse = await adminApi.put(`${apiBaseUrl}/api/editorial/articles/${created.id}`, {
    data: editorialRequest({
      title: 'Updated health editorial title',
      body: 'This updated health-check editorial body is long enough to pass validation.'
    })
  })
  expect(updateResponse.status(), 'admin PUT /api/editorial/articles/{id}').toBe(200)

  const publishResponse = await adminApi.post(`${apiBaseUrl}/api/editorial/articles/${created.id}/publish`)
  expect(publishResponse.status(), 'admin publish editorial').toBe(200)

  const publicDetailResponse = await request.get(`${apiBaseUrl}/api/public/editorial/articles/${created.id}`)
  expect(publicDetailResponse.status(), 'public GET published editorial detail').toBe(200)

  const archiveResponse = await adminApi.post(`${apiBaseUrl}/api/editorial/articles/${created.id}/archive`)
  expect(archiveResponse.status(), 'admin archive editorial').toBe(200)

  const archivedPublicDetailResponse = await request.get(`${apiBaseUrl}/api/public/editorial/articles/${created.id}`)
  expect(archivedPublicDetailResponse.status(), 'public GET archived editorial detail').toBe(404)

  await adminContext.close()
})

test('reader can use public editorial pages without admin access', async ({ browser }) => {
  /*
   * What: Creates published editorial content through the Admin UI before the
   * Reader checks public pages.
   * How: an isolated admin browser context logs in, creates a draft, publishes it,
   * and then closes so the next context starts as a normal Reader.
   * Why: readers cannot create editorial articles, but public reader pages need
   * real published data to prove feed/detail actions work end to end.
   */
  const title = `Reader Public Editorial ${Date.now()}`
  const adminContext = await browser.newContext()
  const adminPage = await adminContext.newPage()

  await login(adminPage, 'admin-e2e@example.com', 'Password123!')
  await expect(adminPage.getByText('Signed in as E2E Admin (Admin).')).toBeVisible()
  await adminPage.getByRole('link', { name: 'Admin' }).click()
  await adminPage.getByLabel('Title').fill(title)
  await adminPage.getByLabel('Category').selectOption('technology')
  await adminPage.getByLabel('Author').fill('E2E Admin')
  await adminPage.getByLabel('Summary').fill('Published content for a signed-in reader.')
  await adminPage.getByLabel('Body').fill('This public editorial article is opened while authenticated as a Reader.')
  await adminPage.getByRole('button', { name: 'Save draft' }).click()
  await expect(adminPage.getByRole('cell', { name: title })).toBeVisible()
  await adminPage.getByRole('row', { name: new RegExp(title) }).getByRole('button', { name: 'Publish' }).click()
  await expect(adminPage.getByRole('row', { name: new RegExp(`${title} Published`) })).toBeVisible()
  await adminContext.close()

  /*
   * What: Verifies public frontend actions while authenticated as a Reader.
   * How: registers a Reader, clicks the public Editorial navigation link, waits
   * for the public feed/detail API responses, and opens the article detail page.
   * Why: public endpoints should remain accessible to signed-in readers, while
   * Admin-only navigation should stay hidden for the Reader role.
   */
  const readerContext = await browser.newContext()
  const readerPage = await readerContext.newPage()
  await registerReader(readerPage, uniqueEmail('reader-public'), 'Public Reader')

  await expect(readerPage.getByRole('link', { name: 'Personal', exact: true })).toBeVisible()
  await expect(readerPage.getByRole('link', { name: 'Saved', exact: true })).toBeVisible()
  await expect(readerPage.getByRole('link', { name: 'Profile', exact: true })).toBeVisible()
  await expect(readerPage.getByRole('link', { name: 'Admin', exact: true })).toHaveCount(0)

  await Promise.all([
    readerPage.waitForResponse((response) =>
      response.url().includes('/api/public/editorial/articles') && response.status() === 200),
    readerPage.getByRole('link', { name: 'Editorial', exact: true }).click()
  ])

  await expect(readerPage.getByRole('heading', { name: 'Editorial Feed' })).toBeVisible()
  await expect(readerPage.getByRole('heading', { name: title })).toBeVisible()

  await Promise.all([
    readerPage.waitForResponse((response) =>
      response.url().includes('/api/public/editorial/articles/') && response.status() === 200),
    readerPage.getByRole('article').filter({ hasText: title }).getByRole('link', { name: 'Read article' }).click()
  ])

  await expect(readerPage.getByRole('heading', { name: title })).toBeVisible()
  await expect(readerPage.getByText('This public editorial article is opened while authenticated as a Reader.')).toBeVisible()

  await readerPage.getByRole('button', { name: 'Logout' }).click()
  await expect(readerPage.getByRole('link', { name: 'Login' })).toBeVisible()
  await readerContext.close()
})

test('admin can publish an editorial article that guests can read', async ({ page }) => {
  const title = `E2E Editorial ${Date.now()}`

  await login(page, 'admin-e2e@example.com', 'Password123!')
  await expect(page.getByText('Signed in as E2E Admin (Admin).')).toBeVisible()

  await page.getByRole('link', { name: 'Admin' }).click()
  await page.getByLabel('Title').fill(title)
  await page.getByLabel('Category').selectOption('technology')
  await page.getByLabel('Author').fill('E2E Admin')
  await page.getByLabel('Summary').fill('This editorial article was created by Playwright.')
  await page.getByLabel('Body').fill('This is the full body of the Playwright editorial article.')
  await page.getByRole('button', { name: 'Save draft' }).click()

  await expect(page.getByRole('cell', { name: title })).toBeVisible()
  await page.getByRole('row', { name: new RegExp(title) }).getByRole('button', { name: 'Publish' }).click()
  await expect(page.getByRole('row', { name: new RegExp(`${title} Published`) })).toBeVisible()

  await page.getByRole('button', { name: 'Logout' }).click()
  await page.getByRole('link', { name: 'Editorial' }).click()
  await expect(page.getByRole('heading', { name: 'Editorial Feed' })).toBeVisible()
  await page.getByRole('article').filter({ hasText: title }).getByRole('link', { name: 'Read article' }).click()

  await expect(page.getByRole('heading', { name: title })).toBeVisible()
  await expect(page.getByText('This is the full body of the Playwright editorial article.')).toBeVisible()
})
