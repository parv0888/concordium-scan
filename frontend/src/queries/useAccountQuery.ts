﻿import { useQuery, gql } from '@urql/vue'
import { Ref } from 'vue'
import { useComponentState } from '~/composables/useComponentState'
import type { Account } from '~/types/generated'
import type { QueryVariables } from '~/types/queryVariables'

type AccountQueryVariables = {
	afterTx: QueryVariables['after']
	beforeTx: QueryVariables['before']
	firstTx: QueryVariables['first']
	lastTx: QueryVariables['last']
	afterReleaseSchedule: QueryVariables['after']
	beforeReleaseSchedule: QueryVariables['before']
	firstReleaseSchedule: QueryVariables['first']
	lastReleaseSchedule: QueryVariables['last']
	afterAccountStatement: QueryVariables['after']
	beforeAccountStatement: QueryVariables['before']
	firstAccountStatement: QueryVariables['first']
	lastAccountStatement: QueryVariables['last']
}

type AccountByIdResponse = {
	account: Account
}

type AccountByAddressResponse = {
	accountByAddress: Account
}

const AccountQueryFragment = `
accountStatement(
				after: $afterAccountStatement
				before: $beforeAccountStatement
				first: $firstAccountStatement
				last: $lastAccountStatement
			) {
  pageInfo {
    hasNextPage
    hasPreviousPage
    startCursor
    endCursor
		__typename
  }
  nodes {
    reference {
      __typename
      ... on Block {
        blockHash
       }
			... on Transaction {
			 transactionHash
			}
    }
    timestamp
    entryType
    amount
    accountBalance
    __typename
  }
}
transactions(
	after: $afterTx
	before: $beforeTx
	first: $firstTx
	last: $lastTx
) {
	pageInfo {
		hasNextPage
		hasPreviousPage
		startCursor
		endCursor
		__typename
	}
	nodes {
		__typename
		transaction {
			id
			transactionHash
			senderAccountAddress {
				asString
			}
			ccdCost
			block {
				blockSlotTime
			}
			result {
				__typename
			}
			transactionType {
				__typename
				... on AccountTransaction {
					accountTransactionType
				}
				... on CredentialDeploymentTransaction {
					credentialDeploymentTransactionType
				}
				... on UpdateTransaction {
					updateTransactionType
				}
			}
		}
	}
}
id
address {
	asString
}
amount
transactionCount
baker {
	bakerId
	state {
		...on ActiveBakerState {
			__typename
			stakedAmount
		}
		...on RemovedBakerState {
			__typename
			removedAt
		}
	}
}
releaseSchedule {
	totalAmount
	schedule(
		after: $afterReleaseSchedule
		before: $beforeReleaseSchedule
		first: $firstReleaseSchedule
		last: $lastReleaseSchedule
	) {
		pageInfo {
			hasNextPage
			hasPreviousPage
			startCursor
			endCursor
		}
		nodes {
			transaction {
				transactionHash
				senderAccountAddress {
					asString
				}
				ccdCost
				block {
					blockSlotTime
				}
				id
				transactionType {
					__typename
				}
				result {
					__typename
				}
			}
			timestamp
			amount
		}
	}
}
createdAt
__typename
`

const AccountQuery = gql<AccountByIdResponse>`
	query (
		$id: ID!
		$afterTx: String
		$beforeTx: String
		$firstTx: Int
		$lastTx: Int
		$afterReleaseSchedule: String
		$beforeReleaseSchedule: String
		$firstReleaseSchedule: Int
		$lastReleaseSchedule: Int
		$afterAccountStatement: String
		$beforeAccountStatement: String
		$firstAccountStatement: Int
		$lastAccountStatement: Int
	) {
		account(id: $id) {
			${AccountQueryFragment}
		}
	}
`

const AccountQueryByAddress = gql<AccountByAddressResponse>`
	query (
		$address: String!
		$afterTx: String
		$beforeTx: String
		$firstTx: Int
		$lastTx: Int
		$afterReleaseSchedule: String
		$beforeReleaseSchedule: String
		$firstReleaseSchedule: Int
		$lastReleaseSchedule: Int
		$afterAccountStatement: String
		$beforeAccountStatement: String
		$firstAccountStatement: Int
		$lastAccountStatement: Int
	) {
		accountByAddress(accountAddress: $address) {
			${AccountQueryFragment}
		}
	}
`

const getData = (
	responseData: AccountByIdResponse | AccountByAddressResponse | undefined
): Account | undefined => {
	if (!responseData) return undefined

	return 'account' in responseData
		? responseData.account
		: responseData.accountByAddress
}

type QueryParams = (
	| {
			id: Ref<string>
			address?: Ref<string>
	  }
	| {
			address: Ref<string>
			id?: Ref<string>
	  }
) & {
	transactionVariables?: AccountQueryVariables
}

export const useAccountQuery = ({
	id,
	address,
	transactionVariables,
}: QueryParams) => {
	const query = id?.value ? AccountQuery : AccountQueryByAddress
	const identifier = id?.value ? { id: id.value } : { address: address?.value }

	const { data, fetching, error } = useQuery({
		query,
		requestPolicy: 'cache-first',
		variables: {
			...identifier,
			...transactionVariables,
		},
	})

	const dataRef = ref(getData(data.value))

	const componentState = useComponentState<Account | undefined>({
		fetching,
		error,
		data: dataRef,
	})

	watch(
		() => data.value,
		value => (dataRef.value = getData(value))
	)

	return { data: dataRef, error, componentState }
}
