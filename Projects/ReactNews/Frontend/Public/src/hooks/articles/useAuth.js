import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { authKeys } from '../../api/articles/articleKeys.js'
import { newsApi } from '../../api/articles/newsApi.js'

/*
 * What: useAuth owns frontend account/session state.
 * How: it loads /api/auth/me and exposes login/register/logout mutations that update the auth cache.
 * Why: navigation and protected hooks need one shared source of truth for the current user.
 */
export function useAuth() {
  const queryClient = useQueryClient()
  const meQuery = useQuery({
    queryKey: authKeys.me(),
    queryFn: ({ signal }) => newsApi.me({ signal }),
    retry: false
  })

  const loginMutation = useMutation({
    mutationFn: (request) => newsApi.login(request),
    onSuccess: (response) => queryClient.setQueryData(authKeys.me(), response)
  })

  const registerMutation = useMutation({
    mutationFn: (request) => newsApi.register(request),
    onSuccess: (response) => queryClient.setQueryData(authKeys.me(), response)
  })

  const logoutMutation = useMutation({
    mutationFn: () => newsApi.logout(),
    onSuccess: () => {
      queryClient.setQueryData(authKeys.me(), null)
      queryClient.invalidateQueries()
    }
  })

  const updateProfileMutation = useMutation({
    mutationFn: (request) => newsApi.updateProfile(request),
    onSuccess: (response) => queryClient.setQueryData(authKeys.me(), response)
  })

  const changePasswordMutation = useMutation({
    mutationFn: (request) => newsApi.changePassword(request),
    onSuccess: (response) => queryClient.setQueryData(authKeys.me(), response)
  })

  const deleteAccountMutation = useMutation({
    mutationFn: (request) => newsApi.deleteAccount(request),
    onSuccess: () => {
      queryClient.setQueryData(authKeys.me(), null)
      queryClient.invalidateQueries()
    }
  })

  return {
    user: meQuery.data?.user ?? null,
    loading: meQuery.isFetching,
    error: loginMutation.error ?? registerMutation.error ?? updateProfileMutation.error ?? changePasswordMutation.error ?? deleteAccountMutation.error ?? null,
    login: loginMutation.mutate,
    register: registerMutation.mutate,
    logout: logoutMutation.mutate,
    updateProfile: updateProfileMutation.mutate,
    changePassword: changePasswordMutation.mutate,
    deleteAccount: deleteAccountMutation.mutate,
    signingIn: loginMutation.isPending || registerMutation.isPending,
    signingOut: logoutMutation.isPending,
    updatingProfile: updateProfileMutation.isPending,
    changingPassword: changePasswordMutation.isPending,
    deletingAccount: deleteAccountMutation.isPending
  }
}
