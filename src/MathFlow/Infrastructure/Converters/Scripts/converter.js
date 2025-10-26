// Scripts/converter.js
const omml2mathml = require('omml2mathml');
const { DOMParser } = require('xmldom');

/**
 * Converte OMML (Office Math Markup Language) para MathML HTML
 * @param {Function} callback - Callback do Node.js (error-first callback)
 * @param {string} omml - String XML contendo o OMML
 */
module.exports = (callback, omml) => {
    try {
        // Valida entrada
        if (!omml || typeof omml !== 'string') {
            return callback(new Error('OMML string is required'), null);
        }

        // Parse do XML OMML
        const parser = new DOMParser();
        const ommlDom = parser.parseFromString(omml, 'application/xml');

        // Verifica se houve erro no parsing
        const parserError = ommlDom.getElementsByTagName('parsererror');
        if (parserError.length > 0) {
            return callback(new Error('Invalid XML: ' + parserError[0].textContent), null);
        }

        // Converte para MathML
        const mathml = omml2mathml(ommlDom);

        // Remove o atributo display (como estava na API)
        mathml.removeAttribute('display');

        // Retorna o HTML do MathML
        callback(null, mathml.outerHTML);
    } catch (error) {
        // Retorna erro com mais contexto
        callback(new Error(`Conversion failed: ${error.message}`), null);
    }
};