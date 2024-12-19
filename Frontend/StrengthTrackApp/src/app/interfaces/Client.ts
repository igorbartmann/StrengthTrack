export interface ResultViewModel {
    id: number;
    sessionId: number;
    value: number;
}

export interface SessionViewModel {
    id: number;
    clientId: number,
    date: Date;    
    number: number;
    averageResult: number;
    results: ResultViewModel[];
}

export interface ClientViewModel {
    id: number;
    name: string;
    cpf: string;
    sessions: SessionViewModel[];
}

export interface ClientSimpleViewModel {
    id: number,
    name: string;
    cpf: string;
}

export interface ClientInputModel {
    name: string;
    cpf: string;
}

export interface Session {
    id: number;
    date: string;    
    number: number;
    averageResult: number;
    results: string;
}