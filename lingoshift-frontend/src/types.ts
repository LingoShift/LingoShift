export interface SequenceConfig {
    id?: number;
    sequenceName: string;
    sequence: string;
    action: string;
    targetLanguage: string;
    useLLM: boolean;
    showPopup: boolean;
}

export interface SequenceAction {
    value: string;
}

export interface Language {
    code: string;
    name: string;
}