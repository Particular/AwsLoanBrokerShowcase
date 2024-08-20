function getRandomInt(min, max) {
    return min + Math.floor(Math.random() * (max-min));
}

exports.score = async (event) => {
    const min_score = 300;
    const max_score = 900;

    var ssn_regex = new RegExp("^\\d{3}-\\d{2}-\\d{4}$");
    let param = event.queryStringParameters;

    if (ssn_regex.test(param.SSN)) {
        return {
            statusCode: 200,
            request_id: param.RequestId,
            body: {
                SSN: param.SSN,
                score: getRandomInt(min_score, max_score),
                history: getRandomInt(1, 30)
            }
        };
    } else {
        return {
            statusCode: 400,
            request_id: param.RequestId,
            body: {
                SSN: param.SSN
            }
        };
    }
};

