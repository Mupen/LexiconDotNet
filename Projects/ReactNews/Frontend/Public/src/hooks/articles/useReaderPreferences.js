import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useCallback, useMemo } from 'react'
import { newsApi } from '../../api/articles/newsApi.js'
import { readerPreferenceKeys } from '../../api/articles/articleKeys.js'
import { defaultPreferences, loadPreferences, savePreferences } from '../../storage/preferences/preferencesStorage.js'

/*
 * What: useReaderPreferences owns persisted display/feed preferences.
 * How: it loads preferences from the backend, falls back to local defaults while loading, and saves updates through a mutation.
 * Why: the reader should keep theme/font/category choices across runs without waiting for full authentication support.
 */
export function useReaderPreferences(enabled = true) {
  const queryClient = useQueryClient()
  const localFallback = useMemo(() => loadPreferences(), [])

  const preferencesQuery = useQuery({
    queryKey: readerPreferenceKeys.detail(),
    queryFn: ({ signal }) => newsApi.getReaderPreferences({ signal }),
    initialData: localFallback,
    enabled
  })

  const updateMutation = useMutation({
    mutationFn: (preferences) => newsApi.updateReaderPreferences(preferences),
    onSuccess: (savedPreferences) => {
      queryClient.setQueryData(readerPreferenceKeys.detail(), savedPreferences)
      savePreferences(savedPreferences)
    }
  })

  const preferences = {
    ...defaultPreferences,
    ...(preferencesQuery.data ?? localFallback)
  }

  const updatePreferences = useCallback((nextPreferences) => {
    const normalized = {
      ...preferences,
      ...nextPreferences
    }

    queryClient.setQueryData(readerPreferenceKeys.detail(), normalized)
    savePreferences(normalized)
    if (enabled) {
      updateMutation.mutate(normalized)
    }
  }, [enabled, preferences, queryClient, updateMutation])

  return {
    preferences,
    loading: preferencesQuery.isFetching,
    saving: updateMutation.isPending,
    error: preferencesQuery.error ?? updateMutation.error ?? null,
    updatePreferences
  }
}
