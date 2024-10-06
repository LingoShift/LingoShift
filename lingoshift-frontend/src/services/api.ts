import axios from 'axios';
import { SequenceConfig } from "../types.ts";

const api = axios.create({
    baseURL: 'http://localhost:5800/api'  // Ensure this URL matches your backend
});

export const getSequences = async () => {
    const response = await api.get('/settings');
    return response.data;
};

export const addSequence = async (sequence: SequenceConfig) => {
    const response = await api.post('/settings', {
        sequenceName: sequence.sequenceName,
        sequence: sequence.sequence,
        action: sequence.action,
        targetLanguage: sequence.targetLanguage,
        useLLM: sequence.useLLM,
        showPopup: sequence.showPopup
    });
    return response.data;
};

export const updateSequence = async (sequence: SequenceConfig) => {
    const response = await api.put(`/settings/${sequence.sequenceName}`, {
        sequenceName: sequence.sequenceName,
        sequence: sequence.sequence,
        action: sequence.action,
        targetLanguage: sequence.targetLanguage,
        useLLM: sequence.useLLM,
        showPopup: sequence.showPopup
    });
    return response.data;
};

export const removeSequence = async (sequenceName: string) => {
    const response = await api.delete(`/settings/${sequenceName}`);
    return response.data;
};

export const getAvailableActions = async () => {
    const response = await api.get('/actions');
    return response.data;
};

export const getAvailableLanguages = async () => {
    const response = await api.get('/languages');
    return response.data;
};