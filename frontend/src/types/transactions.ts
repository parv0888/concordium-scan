import type { Block } from './blocks'

type AccountTransactionTypes =
	| 'DEPLOY_MODULE'
	| 'INITIALIZE_SMART_CONTRACT_INSTANCE'
	| 'UPDATE_SMART_CONTRACT_INSTANCE'
	| 'SIMPLE_TRANSFER'
	| 'ADD_BAKER'
	| 'REMOVE_BAKER'
	| 'UPDATE_BAKER_STAKE'
	| 'UPDATE_BAKER_RESTAKE_EARNINGS'
	| 'UPDATE_BAKER_KEYS'
	| 'UPDATE_CREDENTIAL_KEYS'
	| 'ENCRYPTED_TRANSFER'
	| 'TRANSFER_TO_ENCRYPTED'
	| 'TRANSFER_TO_PUBLIC'
	| 'TRANSFER_WITH_SCHEDULE'
	| 'UPDATE_CREDENTIALS'
	| 'REGISTER_DATA'
	| 'SIMPLE_TRANSFER_WITH_MEMO'
	| 'ENCRYPTED_TRANSFER_WITH_MEMO'
	| 'TRANSFER_WITH_SCHEDULE_WITH_MEMO'

type CredentialDeploymentTypes = 'INITIAL' | 'NORMAL'

type UpdateTransactionTypes =
	| 'UPDATE_PROTOCOL'
	| 'UPDATE_ELECTION_DIFFICULTY'
	| 'UPDATE_EURO_PER_ENERGY'
	| 'UPDATE_MICRO_GTU_PER_EURO'
	| 'UPDATE_FOUNDATION_ACCOUNT'
	| 'UPDATE_MINT_DISTRIBUTION'
	| 'UPDATE_TRANSACTION_FEE_DISTRIBUTION'
	| 'UPDATE_GAS_REWARDS'
	| 'UPDATE_BAKER_STAKE_THRESHOLD'
	| 'UPDATE_ADD_ANONYMITY_REVOKER'
	| 'UPDATE_ADD_IDENTITY_PROVIDER'
	| 'UPDATE_ROOT_KEYS'
	| 'UPDATE_LEVEL1_KEYS'
	| 'UPDATE_LEVEL2_KEYS'

export type AccountTransaction = {
	__typename: 'AccountTransaction'
	accountTransactionType: AccountTransactionTypes
}

export type CredentialDeploymentTransaction = {
	__typename: 'CredentialDeploymentTransaction'
	credentialDeploymentTransactionType: CredentialDeploymentTypes
}

export type UpdateTransaction = {
	__typename: 'UpdateTransaction'
	updateTransactionType: UpdateTransactionTypes
}

export type TransactionType =
	| AccountTransaction
	| UpdateTransaction
	| CredentialDeploymentTransaction

type AccountCreatedEvent = {
	__typename: 'AccountCreated'
	address: string
}

type CredentialDeployed = {
	__typename: 'CredentialDeployed'
	regId: string
	accountAddress: string
}

type AccountAddress = {
	__typename: 'AccountAddress'
	address: string
}

type ContractAddress = {
	__typename: 'ContractAddress'
	index: string
	subIndex: string
}

export type TransferAddress = AccountAddress | ContractAddress

type TransferredEvent = {
	__typename: 'Transferred'
	from: TransferAddress
	to: TransferAddress
}

export type TransactionSuccessfulEvent =
	| AccountCreatedEvent
	| CredentialDeployed
	| TransferredEvent

type TransactionSuccessful = {
	successful: true
	events: {
		nodes: TransactionSuccessfulEvent[]
	}
}

type TransactionRejected = {
	successful: false
}

type TransactionResult = TransactionSuccessful | TransactionRejected

export type Transaction = {
	id: string
	blockHeight: number
	blockHash: string
	transactionHash: string
	senderAccountAddress: string
	ccdCost: number
	block: Block
	result: TransactionResult
	transactionType: TransactionType
}
